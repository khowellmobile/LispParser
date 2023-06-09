/*
<Program> ::= {<SExpr>}
<SExpr> ::= <Atom> | <List>
<List> ::= () | ( <Seq> )
<Seq> ::= <SExpr> <Seq> | <SExpr>
<Atom> ::= ID | INT | REAL | STRING
*/



using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LispishParser
{

    public class Parser {
        List<Node> tokens = new List<Node>();
        int cur = 0;

        public Parser(Node [] tokens) {
            this.tokens = new List<Node>(tokens);
            this.tokens.Add(new Node(Symbols.INVALID, ""));
        }

        public Node ParseProgram() {
            // <Program> ::= {<SExpr>}
            var children = new List<Node>();

            while (tokens[cur].symbol != Symbols.INVALID){
                children.Add(ParseSExpr());
            }
            return new Node(Symbols.Program, children.ToArray());
        }

        public Node ParseSExpr() {
           // <SExpr> ::= <Atom> | <List>
           var children = new List<Node>();

           if (trueAtomFalseList()) {
                children.Add(ParseAtom());
                return new Node(Symbols.SExpr, children.ToArray());
           } else {
                children.Add(ParseList());
                return new Node(Symbols.SExpr, children.ToArray());
           }
        }

        public Node ParseList() {
            // <List> ::= () | ( <Seq> )
            if (tokens[cur].text == "(" && tokens[cur + 1].text == ")") {
                var lparen = ParseLiteral("(");
                var rparen = ParseLiteral(")");
                return new Node(Symbols.List, lparen, rparen);
            } else {
                var lparen = ParseLiteral("(");
                var seq = ParseSeq();
                var rparen = ParseLiteral(")");
                return new Node(Symbols.List, lparen, seq, rparen);
            }
        }

        public Node ParseSeq() {
            //<Seq> ::= <SExpr> <Seq> | <SExpr>
            var children = new List<Node>();
            children.Add(ParseSExpr());

            if (tokens[cur].text == ")") {
                return new Node(Symbols.Seq, children.ToArray());
            } else {
                children.Add(ParseSeq());
                return new Node(Symbols.Seq, children.ToArray());
            }
        }

        public Node ParseAtom() {
            // <Atom> ::= ID | INT | REAL | STRING
            if(tokens[cur].symbol == Symbols.ID || tokens[cur].symbol == Symbols.INT ||
                tokens[cur].symbol == Symbols.REAL || tokens[cur].symbol == Symbols.STRING) {
                    return new Node(Symbols.Atom, tokens[cur++]);
            } else {
                throw new Exception("Syntax Error");
            }
        }

        public Node ParseLiteral(string lit) {
            if (tokens[cur].text == lit) {
                return tokens[cur++];
            } else {
                throw new Exception("Syntax error");
            }
        }

        public bool trueAtomFalseList() {
            if(tokens[cur].symbol == Symbols.ID || tokens[cur].symbol == Symbols.INT ||
                tokens[cur].symbol == Symbols.REAL || tokens[cur].symbol == Symbols.STRING) {
                    return true;
            } else {
                return false;
            }
        }

    }


    public enum Symbols{
        INVALID,
        ID,
        INT,
        STRING,
        LITERAL,
        REAL,


        Program,
        SExpr,
        List,
        Seq,
        Atom
    }
    public class Node
    {
        public Symbols symbol;
        public string text = "";

        List<Node> children = new List<Node>();

        public Node(Symbols symbol, string text){
            this.symbol = symbol;
            this.text = text;
        }

        public Node(Symbols symbol, params Node[] children){
            this.symbol = symbol;
            this.text = "";
            this.children = new List<Node>(children);
        }

        public void Print(String prefix = "") {
            var whiteSpace = "";
            if (text == "") {
                whiteSpace = "";
            } else {
                whiteSpace = " ";
            }
            Console.WriteLine($"{prefix}{symbol.ToString().PadRight(39-prefix.Length)} {whiteSpace}{text}");
            foreach (var child in children){
                child.Print(prefix+"  ");
            }
        }
    }

    /*
    *   LITERAL = [\(\)]
    *   REAL = [+-]?[0-9]*\.[0-9]+
    *   INT = [+-]?[0-9]+
    *   STRING = "(?>\\.|[^\\"])*".  Multiline strings are not allowed.
    *   ID = [^\s"\(\)]+
    *   Anything else other than whitespace is an error ( INVALID)
    *
    */
    static public List<Node> Tokenize(String src)
    {
        var result = new List<Node>();

        int pos = 0;

        var WS = new Regex(@"\G\s");
        var LITERAL = new Regex(@"\G[\(\)]");
        var REAL = new Regex(@"\G[+-]?[0-9]*\.[0-9]+");
        var INT = new Regex(@"\G[+-]?[0-9]+");
        var STRING = new Regex(@"\G""(?>\\.|[^\\""])*""");
        var ID = new Regex(@"\G[^\s""\(\)]+");
        Match m;


        while(pos < src.Length) {
            if ((m = WS.Match(src, pos)).Success) {
                pos += m.Length;
            } else if ((m = REAL.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.REAL, m.Value));
                pos += m.Length;
            } else if ((m = INT.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.INT, m.Value));
                pos += m.Length;
            } else if ((m = STRING.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.STRING, m.Value));
                pos += m.Length;
            } else if ((m = ID.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.ID, m.Value));
                pos += m.Length;
            } else if ((m = LITERAL.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.LITERAL, m.Value));
                pos += m.Length;
            } else {
                throw new Exception("Lexer error");
            }
        }

        return result;
    }

    static public Node Parse(Node[] tokens)
    {
        var p = new Parser(tokens);
        var tree = p.ParseProgram();
        return tree;
    }

    static private void CheckString(string lispcode)
    {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.symbol,-21}: {node.text}");
            }
            Console.WriteLine(new String('-', 50));

            Node parseTree = Parse(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
        //Here are some strings to test on in 
        //your debugger. You should comment 
        //them out before submitting!

        // CheckString(@"(define foo 3)");
        // CheckString(@"(define foo ""bananas"")");
        // CheckString(@"(define foo ""Say \\""Chease!\"""")");
        // CheckString(@"(define foo ""Say \\""Chease!\)");
        // CheckString(@"(+ 3 4)");      
        // CheckString(@"(+ 3.14 (* 4 7))");
        // CheckString(@"(+ 3.14 (* 4 7)");

        CheckString(Console.In.ReadToEnd());
    }
}

