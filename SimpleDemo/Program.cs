using System;
using F;
using LC;
namespace SimpleDemo {
    class Program {
        static void Main(string[] args) {
			// our expression
			var exp = "foo|(bar)+|baz";
			Console.WriteLine(exp);
			
			// parse it
			var fa = FFA.Parse(exp);
			// write it back as an equivalent regular expression
			Console.WriteLine(fa);
			// run IsMatch
			Console.WriteLine("IsMatch(\"foo\") = {0}", fa.IsMatch("foo"));
			// run IsMatch (DFA table)
			Console.WriteLine("IsMatch(dfa, \"barbar\") = {0}", FFA.IsMatch(fa.ToDfaTable(), "barbar"));
			
			// run searches
			var srch = "abcde foo fghij barbar klmnop baz";
			var lc = LexContext.Create(srch);
			Console.WriteLine("Search(\"{0}\")", srch);
			var pos = 0L;
			// run Search
			while (-1 < (pos = fa.Search(lc))) {
				Console.WriteLine("\t{0} @ {1}", lc.GetCapture(), pos);
			}
			lc = LexContext.Create(srch);
			Console.WriteLine("Search(dfa, \"{0}\")", srch);
			pos = 0L;
			// run Search (DFA table)
			while (-1 < (pos = FFA.Search(fa.ToDfaTable(), lc))) {
				Console.WriteLine("\t{0} @ {1}", lc.GetCapture(), pos);
			}
			// C identifier:
			var ident = FFA.Parse("[A-Z_a-z][A-Z_a-z0-9]*");
			var opts = new FFA.DotGraphOptions();
			// don't need to see accept symbol ids
			opts.HideAcceptSymbolIds = true;
			// render it to a jpg in the project directory
			ident.RenderToFile(@"..\..\..\ident.jpg",opts);
		}
    }
}
