using System;
using F;
using LC;
namespace SimpleDemo {
    class Program {
        static void Main(string[] args) {
			var exp = "foo|(bar)+|baz";
			Console.WriteLine(exp);
			var fa = FFA.Parse(exp);
			Console.WriteLine(fa);
			Console.WriteLine("IsMatch(\"foo\") = {0}", fa.IsMatch("foo"));
			Console.WriteLine("IsMatch(dfa, \"barbar\") = {0}", FFA.IsMatch(fa.ToDfaTable(), "barbar"));
			var srch = "abcde foo fghij barbar klmnop baz";
			var lc = LexContext.Create(srch);
			Console.WriteLine("Search(\"{0}\")", srch);
			var pos = 0L;
			while (-1 < (pos = fa.Search(lc))) {
				Console.WriteLine("\t{0} @ {1}", lc.GetCapture(), pos);
			}
			lc = LexContext.Create(srch);
			Console.WriteLine("Search(dfa, \"{0}\")", srch);
			pos = 0L;
			while (-1 < (pos = FFA.Search(fa.ToDfaTable(), lc))) {
				Console.WriteLine("\t{0} @ {1}", lc.GetCapture(), pos);
			}
		}
    }
}
