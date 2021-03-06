// Portions of this code adopted from Fare, itself adopted from brics
// original copyright notice included.
// This is the only file this applies to.

/*
 * 
* The MIT License (MIT)
* 
* Copyright (c) 2013 Nikos Baxevanis
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE
* 
 * dk.brics.automaton
 * 
 * Copyright (c) 2001-2011 Anders Moeller
 * All rights reserved.
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://www.brics.dk/automaton/
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using LC;
namespace F
{
	/// <summary>
	/// Represents a single transition in a state
	/// </summary>
#if FFALIB
	public
#endif
	partial struct FFATransition
	{
		/// <summary>
		/// The minimum value in the range
		/// </summary>
		public int Min;
		/// <summary>
		/// The maximum value in the range
		/// </summary>
		public int Max;
		/// <summary>
		/// The destination state
		/// </summary>
		public FFA To;
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="min">The minimum value in the range</param>
		/// <param name="max">The maximum value in the range</param>
		/// <param name="to">The destination state</param>
		public FFATransition(int min, int max, FFA to)
		{
			Min = min;
			Max = max;
			To = to;
		}

	}
	/// <summary>
	/// Represents a single state and/or the root state in a state machine.
	/// </summary>
#if FFALIB
	public
#endif
	partial class FFA
	{
		/// <summary>
		/// Indicates whether this state is deterministic.
		/// </summary>
		public bool IsDeterministic { get; private set; } = true;
		/// <summary>
		/// Indicates whether this state is accepting.
		/// </summary>
		public bool IsAccepting { get; set; } = false;
		/// <summary>
		/// Indicates the integer accept symbol.
		/// </summary>
		/// <remarks>-1 is reserved and should not be used.</remarks>
		public int AcceptSymbol { get; set; } = 0;
		/// <summary>
		/// Indicates an application defined tag for this state
		/// </summary>
		public int Tag { get; set; } = 0;
		/// <summary>
		/// Indicates the list of transitions for this state
		/// </summary>
		public readonly IList<FFATransition> Transitions = new List<FFATransition>();
		/// <summary>
		/// Constructs a new instance of a state
		/// </summary>
		/// <param name="isAccepting">Indicates whether the state is accepting.</param>
		/// <param name="acceptSymbol">Indicates the integer accept symbol for this state.</param>
		public FFA(bool isAccepting = false, int acceptSymbol = 0)
		{
			IsAccepting = isAccepting;
			AcceptSymbol = acceptSymbol;
		}
		/// <summary>
		/// Indicates if this state has no outgoing transitions.
		/// </summary>
		public bool IsFinal { get { return 0 == Transitions.Count; } }
		/// <summary>
		/// Adds an epsilon transition to this state.
		/// </summary>
		/// <param name="to">The destination state</param>
		/// <remarks>Epsilon transitions are flattened into the machine as soon as they are added so they cannot be readily extracted in the same form they were inserted.</remarks>
		public void AddEpsilon(FFA to)
		{
			if (to.IsAccepting && !IsAccepting)
			{
				IsAccepting = true;
				AcceptSymbol = to.AcceptSymbol;
			}
			for (int ic = to.Transitions.Count, i = 0; i < ic; ++i)
			{
				Transitions.Add(to.Transitions[i]);
			}
			IsDeterministic = false;
		}
		/// <summary>
		/// Fills a list with the set of all states reachable from this state along the arrows, including itself as the first state.
		/// </summary>
		/// <param name="result">An optional list to fill. If not indicated, one will be created.</param>
		/// <returns>The filled list.</returns>
		public IList<FFA> FillClosure(IList<FFA> result = null)
		{
			if (null == result)
				result = new List<FFA>();
			if (result.Contains(this))
				return result;
			result.Add(this);
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				t.To.FillClosure(result);
			}
			return result;
		}
		/// <summary>
		/// Fills a list with the set of all states in the machine that are accepting.
		/// </summary>
		/// <param name="result">An optional list to fill. If not indicated, one will be created.</param>
		/// <returns>The filled list.</returns>
		public IList<FFA> FillAcceptingStates(IList<FFA> result = null)
		{
			return FillAcceptingStates(FillClosure(), result);
		}
		/// <summary>
		/// Fills a list with the set of all states in the machine that are accepting.
		/// </summary>
		/// <param name="closure">The precomputed closure to use.</param>
		/// <param name="result">An optional list to fill. If not indicated, one will be created.</param>
		/// <returns>The filled list.</returns>
		public static IList<FFA> FillAcceptingStates(IList<FFA> closure, IList<FFA> result = null)
		{
			if (null == result)
				result = new List<FFA>();
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				if (fa.IsAccepting)
					result.Add(fa);
			}
			return result;
		}
		/// <summary>
		/// Fills a dictionary with packed input ranges keyed by destination state.
		/// </summary>
		/// <param name="result">A dictionary to fill. If not indicated, one will be created.</param>
		/// <returns>A dictionary filled with the ranges grouped by state</returns>
		public IDictionary<FFA, int[]> FillInputTransitionRangesGroupedByState(IDictionary<FFA, int[]> result = null)
		{
			var working = new Dictionary<FFA, List<KeyValuePair<int, int>>>();
			foreach (var trns in Transitions)
			{
				List<KeyValuePair<int, int>> l;
				if (!working.TryGetValue(trns.To, out l))
				{
					l = new List<KeyValuePair<int, int>>();
					working.Add(trns.To, l);
				}
				l.Add(new KeyValuePair<int, int>(trns.Min, trns.Max));
			}
			if (null == result)
				result = new Dictionary<FFA, int[]>();
			foreach (var item in working)
			{
				item.Value.Sort((x, y) => { var c = x.Key.CompareTo(y.Key); if (0 != c) return c; return x.Value.CompareTo(y.Value); });
				_NormalizeSortedRangeList(item.Value);
				result.Add(item.Key, _FromPairs(item.Value));
			}
			return result;
		}
		static void _NormalizeSortedRangeList(IList<KeyValuePair<int, int>> pairs)
		{

			var or = default(KeyValuePair<int, int>);
			for (int i = 1; i < pairs.Count; ++i)
			{
				if (pairs[i - 1].Value + 1 >= pairs[i].Key)
				{
					var nr = new KeyValuePair<int, int>(pairs[i - 1].Key, pairs[i].Value);
					pairs[i - 1] = or = nr;
					pairs.RemoveAt(i);
					--i; // compensated for by ++i in for loop
				}
			}
		}
		/// <summary>
		/// Creates a deep copy of this machine, meaning this state and any referenced states
		/// </summary>
		/// <returns>A copy of this machine that is its equivalent.</returns>
		public FFA Clone() { return Clone(FillClosure()); }
        /// <summary>
        /// Creates a deep copy of this machine, meaning this state and any referenced states
        /// </summary>
        /// <param name="closure">The precomputed closure used to clone.</param>
        /// <returns>A copy of this machine that is its equivalent.</returns>
        public static FFA Clone(IList<FFA> closure) {
            var nclosure = new FFA[closure.Count];
            for (var i = 0; i < nclosure.Length; i++) {
                var fa = closure[i];
                var nfa = new FFA();
                nfa.IsAccepting = fa.IsAccepting;
                nfa.AcceptSymbol = fa.AcceptSymbol;
                nfa.IsDeterministic = fa.IsDeterministic;
                nclosure[i] = nfa;
            }
            for (var i = 0; i < nclosure.Length; i++) {
                var fa = closure[i];
                var nfa = nclosure[i];
                for (int jc = fa.Transitions.Count, j = 0; j < jc; ++j) {
                    var t = fa.Transitions[j];
                    nfa.Transitions.Add(new FFATransition(t.Min, t.Max, nclosure[closure.IndexOf(t.To)]));
                }
            }
            return nclosure[0];
        }
		/// <summary>
		/// Builds a machine that matches a literal sequence.
		/// </summary>
		/// <param name="string">The inputs to accept. Typically, these are UTF32 codepoints.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new machine that matches the specified string</returns>
        public static FFA Literal(IEnumerable<int> @string, int accept = 0)
		{
			var result = new FFA();
			var current = result;
			foreach (var ch in @string)
			{
				current.IsAccepting = false;
				var fa = new FFA();
				fa.IsAccepting = true;
				fa.AcceptSymbol = accept;
				current.Transitions.Add(new FFATransition(ch, ch, fa));
				current = fa;
			}
			return result;
		}
		/// <summary>
		/// Builds a machine that matches one of a set of inputs.
		/// </summary>
		/// <param name="ranges">The input ranges to accept. Typically, these are UTF32 codepoints.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new machine that matches the specified set of ranges.</returns>
		public static FFA Set(IEnumerable<KeyValuePair<int, int>> ranges, int accept = 0)
		{
			var result = new FFA();
			var final = new FFA(true, accept);
			var pairs = new List<KeyValuePair<int, int>>(ranges);
			pairs.Sort((x, y) => { var c = x.Key.CompareTo(y.Key); if (0 != c) return c; return x.Value.CompareTo(y.Value); });
			foreach (var pair in pairs)
				result.Transitions.Add(new FFATransition(pair.Key, pair.Value, final));
			return result;
		}
		/// <summary>
		/// Builds a machine that is the concatenation of several machines
		/// </summary>
		/// <param name="exprs">The machines to concatenate.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new machine that matches the specified contiguous sequence of expressions.</returns>
		public static FFA Concat(IEnumerable<FFA> exprs, int accept = 0)
		{
			FFA result = null, left = null, right = null;
			foreach (var val in exprs)
			{
				if (null == val) continue;
				//Debug.Assert(null != val.FirstAcceptingState);
				var nval = val.Clone();
				//Debug.Assert(null != nval.FirstAcceptingState);
				if (null == left)
				{
					if (null == result)
						result = nval;
					left = nval;
					//Debug.Assert(null != left.FirstAcceptingState);
					continue;
				}
				if (null == right)
				{
					right = nval;
				}

				//Debug.Assert(null != left.FirstAcceptingState);
				nval = right.Clone();
				_Concat(left, nval);
				right = null;
				left = nval;

				//Debug.Assert(null != left.FirstAcceptingState);

			}
			if (null != right)
			{
				var acc = right.FillAcceptingStates();
				for (int ic = acc.Count, i = 0; i < ic; ++i)
					acc[i].AcceptSymbol = accept;
			}
			else
			{
				var acc = result.FillAcceptingStates();
				for (int ic = acc.Count, i = 0; i < ic; ++i)
					acc[i].AcceptSymbol = accept;
			}
			return result;
		}
		static void _Concat(FFA lhs, FFA rhs)
		{
			//Debug.Assert(lhs != rhs);
			var acc = lhs.FillAcceptingStates();
			for (int ic = acc.Count, i = 0; i < ic; ++i)
			{
				var f = acc[i];
				//Debug.Assert(null != rhs.FirstAcceptingState);
				f.IsAccepting = false;
				f.AddEpsilon(rhs);
				//Debug.Assert(null!= lhs.FirstAcceptingState);
			}
		}
		/// <summary>
		/// Builds a machine that is the union of several machines
		/// </summary>
		/// <param name="exprs">The machines to union.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new machine that matches any one of the specified expressions.</returns>
		public static FFA Or(IEnumerable<FFA> exprs, int accept = 0)
		{
			var result = new FFA();
			var final = new FFA(true, accept);
			foreach (var fa in exprs)
			{
				if (null != fa)
				{
					var nfa = fa.Clone();
					result.AddEpsilon(nfa);
					var acc = nfa.FillAcceptingStates();
					for (int ic = acc.Count, i = 0; i < ic; ++i)
					{
						var nffa = acc[i];
						nffa.IsAccepting = false;
						nffa.AddEpsilon(final);
					}
				}
				else result.AddEpsilon(final);
			}
			return result;
		}
		/// <summary>
		/// Creates a machine that matches the expression or the empty string
		/// </summary>
		/// <param name="expr">The expression</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new machine that matches the expression or the empty string</returns>
		public static FFA Optional(FFA expr, int accept = 0)
		{
			var result = expr.Clone();
			var acc = result.FillAcceptingStates();
			for (int ic = acc.Count, i = 0; i < ic; ++i)
			{
				var fa = acc[i];
				fa.IsAccepting = true;
				fa.AcceptSymbol = accept;
				result.AddEpsilon(fa);
			}
			return result;
		}
		/// <summary>
		/// Creates a machine that matches the specified repeating sequence of an expression
		/// </summary>
		/// <param name="expr">The expression</param>
		/// <param name="minOccurs">The minimum number of occurrances to match.</param>
		/// <param name="maxOccurs">The maximum number of occurrances to match, or 0 for no maximum.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new machine that matches the specified repeating sequence of an expression</returns>
		public static FFA Repeat(FFA expr, int minOccurs = -1, int maxOccurs = -1, int accept = 0)
		{
			expr = expr.Clone();
			if (minOccurs > 0 && maxOccurs > 0 && minOccurs > maxOccurs)
				throw new ArgumentOutOfRangeException(nameof(maxOccurs));
			FFA result;
			switch (minOccurs)
			{
				case -1:
				case 0:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = new FFA(true, accept);
							result.AddEpsilon(expr);
							foreach (var afa in expr.FillAcceptingStates())
							{
								afa.AddEpsilon(result);
							}
							return result;
						case 1:
							result = Optional(expr, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						default:
							var l = new List<FFA>();
							expr = Optional(expr);
							l.Add(expr);
							for (int i = 1; i < maxOccurs; ++i)
							{
								l.Add(expr.Clone());
							}
							result = Concat(l, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
				case 1:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = Concat(new FFA[] { expr, Repeat(expr, 0, 0, accept) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							//Debug.Assert(null != expr.FirstAcceptingState);
							return expr;
						default:
							result = Concat(new FFA[] { expr, Repeat(expr.Clone(), 0, maxOccurs - 1) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
				default:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = Concat(new FFA[] { Repeat(expr, minOccurs, minOccurs, accept), Repeat(expr, 0, 0, accept) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							throw new ArgumentOutOfRangeException(nameof(maxOccurs));
						default:
							if (minOccurs == maxOccurs)
							{
								var l = new List<FFA>();
								l.Add(expr);
								//Debug.Assert(null != expr.FirstAcceptingState);
								for (int i = 1; i < minOccurs; ++i)
								{
									var e = expr.Clone();
									//Debug.Assert(null != e.FirstAcceptingState);
									l.Add(e);
								}
								result = Concat(l, accept);
								//Debug.Assert(null != result.FirstAcceptingState);
								return result;
							}
							result = Concat(new FFA[] { Repeat(expr.Clone(), minOccurs, minOccurs, accept), Repeat(Optional(expr.Clone()), maxOccurs - minOccurs, maxOccurs - minOccurs, accept) }, accept);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
			}
			// should never get here
			throw new NotImplementedException();
		}
		/// <summary>
		/// Makes a new case insenstive expression based on an expression
		/// </summary>
		/// <param name="expr">The expression.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <returns>A new expression that matches the expression without regard to case</returns>
		public static FFA CaseInsensitive(FFA expr, int accept = 0)
		{
			var result = expr.Clone();
			var closure = new List<FFA>();
			result.FillClosure(closure);
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				var t = new List<FFATransition>(fa.Transitions);
				fa.Transitions.Clear();
				foreach (var trns in t)
				{
					var f = char.ConvertFromUtf32(trns.Min);
					var l = char.ConvertFromUtf32(trns.Max);
					if (char.IsLower(f, 0))
					{
						if (!char.IsLower(l, 0))
							throw new NotSupportedException("Attempt to make an invalid range case insensitive");
						fa.Transitions.Add(new FFATransition(trns.Min, trns.Max, trns.To));
						f = f.ToUpperInvariant();
						l = l.ToUpperInvariant();
						fa.Transitions.Add(new FFATransition(char.ConvertToUtf32(f, 0), char.ConvertToUtf32(l, 0), trns.To));

					}
					else if (char.IsUpper(f, 0))
					{
						if (!char.IsUpper(l, 0))
							throw new NotSupportedException("Attempt to make an invalid range case insensitive");
						fa.Transitions.Add(new FFATransition(trns.Min, trns.Max, trns.To));
						f = f.ToLowerInvariant();
						l = l.ToLowerInvariant();
						fa.Transitions.Add(new FFATransition(char.ConvertToUtf32(f, 0), char.ConvertToUtf32(l, 0), trns.To));
					}
					else
					{
						fa.Transitions.Add(new FFATransition(trns.Min, trns.Max, trns.To));
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Parses a regular expression
		/// </summary>
		/// <param name="input">The input expression text.</param>
		/// <param name="accept">The accept symbol id.</param>
		/// <param name="line">The line where the parsing is assumed to have started.</param>
		/// <param name="column">The column where the parsing is assumed to have started.</param>
		/// <param name="position">The position where the parsing is assumed to have started.</param>
		/// <param name="fileOrUrl">The source document that contains the input.</param>
		/// <returns>A new machine that matches the specified regular expression</returns>
		public static FFA Parse(IEnumerable<char> input, int accept = 0, int line = 1, int column = 1, long position = 0, string fileOrUrl = null)
		{
			var lc = LexContext.Create(input);
			lc.EnsureStarted();
			lc.SetLocation(line, column, position, fileOrUrl);
			var result = Parse(lc, accept);
			return result;
		}
		internal static FFA Parse(LexContext pc, int accept = 0)
		{

			FFA result = null, next = null;
			int ich;
			pc.EnsureStarted();
			while (true)
			{
				switch (pc.Current)
				{
					case -1:
						result = result.ToMinimized();
						return result;
					case '.':
						var dot = FFA.Set(new KeyValuePair<int, int>[] { new KeyValuePair<int, int>(0, 0x10ffff) }, accept);
						if (null == result)
							result = dot;
						else
						{
							result = FFA.Concat(new FFA[] { result, dot }, accept);
						}
						pc.Advance();
						result = _ParseModifier(result, pc, accept);
						break;
					case '\\':

						pc.Advance();
						pc.Expecting();
						var isNot = false;
						switch (pc.Current)
						{
							case 'P':
								isNot = true;
								goto case 'p';
							case 'p':
								pc.Advance();
								pc.Expecting('{');
								var uc = new StringBuilder();
								int uli = pc.Line;
								int uco = pc.Column;
								long upo = pc.Position;
								while (-1 != pc.Advance() && '}' != pc.Current)
									uc.Append((char)pc.Current);
								pc.Expecting('}');
								pc.Advance();
								int uci = 0;
								switch (uc.ToString())
								{
									case "Pe":
										uci = 21;
										break;
									case "Pc":
										uci = 19;
										break;
									case "Cc":
										uci = 14;
										break;
									case "Sc":
										uci = 26;
										break;
									case "Pd":
										uci = 19;
										break;
									case "Nd":
										uci = 8;
										break;
									case "Me":
										uci = 7;
										break;
									case "Pf":
										uci = 23;
										break;
									case "Cf":
										uci = 15;
										break;
									case "Pi":
										uci = 22;
										break;
									case "Nl":
										uci = 9;
										break;
									case "Zl":
										uci = 12;
										break;
									case "Ll":
										uci = 1;
										break;
									case "Sm":
										uci = 25;
										break;
									case "Lm":
										uci = 3;
										break;
									case "Sk":
										uci = 27;
										break;
									case "Mn":
										uci = 5;
										break;
									case "Ps":
										uci = 20;
										break;
									case "Lo":
										uci = 4;
										break;
									case "Cn":
										uci = 29;
										break;
									case "No":
										uci = 10;
										break;
									case "Po":
										uci = 24;
										break;
									case "So":
										uci = 28;
										break;
									case "Zp":
										uci = 13;
										break;
									case "Co":
										uci = 17;
										break;
									case "Zs":
										uci = 11;
										break;
									case "Mc":
										uci = 6;
										break;
									case "Cs":
										uci = 16;
										break;
									case "Lt":
										uci = 2;
										break;
									case "Lu":
										uci = 0;
										break;
								}
								if (isNot)
								{
									next = FFA.Set(_ToPairs(CharacterClasses.UnicodeCategories[uci]), accept);
								}
								else
									next = FFA.Set(_ToPairs(CharacterClasses.NotUnicodeCategories[uci]), accept);
								break;
							case 'd':
								next = FFA.Set(_ToPairs(CharacterClasses.digit), accept);
								pc.Advance();
								break;
							case 'D':
								next = FFA.Set(_NotRanges(CharacterClasses.digit), accept);
								pc.Advance();
								break;

							case 's':
								next = FFA.Set(_ToPairs(CharacterClasses.space), accept);
								pc.Advance();
								break;
							case 'S':
								next = FFA.Set(_NotRanges(CharacterClasses.space), accept);
								pc.Advance();
								break;
							case 'w':
								next = FFA.Set(_ToPairs(CharacterClasses.word), accept);
								pc.Advance();
								break;
							case 'W':
								next = FFA.Set(_NotRanges(CharacterClasses.word), accept);
								pc.Advance();
								break;
							default:
								if (-1 != (ich = _ParseEscapePart(pc)))
								{
									next = FFA.Literal(new int[] { ich }, accept);

								}
								else
								{
									pc.Expecting(); // throw an error
									return null; // doesn't execute
								}
								break;
						}
						next = _ParseModifier(next, pc, accept);
						if (null != result)
						{
							result = FFA.Concat(new FFA[] { result, next }, accept);
						}
						else
							result = next;
						break;
					case ')':
						result = result.ToMinimized();
						return result;
					case '(':
						pc.Advance();
						pc.Expecting();
						next = Parse(pc, accept);
						pc.Expecting(')');
						pc.Advance();
						next = _ParseModifier(next, pc, accept);
						if (null == result)
							result = next;
						else
						{
							result = FFA.Concat(new FFA[] { result, next }, accept);
						}
						break;
					case '|':
						if (-1 != pc.Advance())
						{
							next = Parse(pc, accept);
							result = FFA.Or(new FFA[] { result, next }, accept);
						}
						else
						{
							result = FFA.Optional(result, accept);
						}
						break;
					case '[':
						var seti = _ParseSet(pc);
						IEnumerable<KeyValuePair<int, int>> set;
						if (seti.Key)
							set = _NotRanges(seti.Value);
						else
							set = _ToPairs(seti.Value);
						next = FFA.Set(set, accept);
						next = _ParseModifier(next, pc, accept);

						if (null == result)
							result = next;
						else
						{
							result = FFA.Concat(new FFA[] { result, next }, accept);

						}
						break;
					default:
						ich = pc.Current;
						if (char.IsHighSurrogate((char)ich))
						{
							if (-1 == pc.Advance())
								throw new ExpectingException("Expecting low surrogate in Unicode stream", pc.Line, pc.Column, pc.Position, pc.FileOrUrl, "low-surrogate");
							ich = char.ConvertToUtf32((char)ich, (char)pc.Current);
						}
						next = FFA.Literal(new int[] { ich }, accept);
						pc.Advance();
						next = _ParseModifier(next, pc, accept);
						if (null == result)
							result = next;
						else
						{
							result = FFA.Concat(new FFA[] { result, next }, accept);
						}
						break;
				}
			}
		}

		static KeyValuePair<bool, int[]> _ParseSet(LexContext pc)
		{
			var result = new List<int>();
			pc.EnsureStarted();
			pc.Expecting('[');
			pc.Advance();
			pc.Expecting();
			var isNot = false;
			if ('^' == pc.Current)
			{
				isNot = true;
				pc.Advance();
				pc.Expecting();
			}
			var firstRead = true;
			int firstChar = '\0';
			var readFirstChar = false;
			var wantRange = false;
			while (-1 != pc.Current && (firstRead || ']' != pc.Current))
			{
				if (!wantRange)
				{
					// can be a single char,
					// a range
					// or a named character class
					if ('[' == pc.Current) // named char class
					{
						pc.Advance();
						pc.Expecting();
						if (':' != pc.Current)
						{
							firstChar = '[';
							readFirstChar = true;
						}
						else
						{
							pc.Advance();
							pc.Expecting();
							var ll = pc.CaptureBuffer.Length;
							if (!pc.TryReadUntil(':', false))
								throw new ExpectingException("Expecting character class", pc.Line, pc.Column, pc.Position, pc.FileOrUrl);
							pc.Expecting(':');
							pc.Advance();
							pc.Expecting(']');
							pc.Advance();
							var cls = pc.GetCapture(ll);
							int[] ranges;
							if (!CharacterClasses.Known.TryGetValue(cls, out ranges))
								throw new ExpectingException("Unknown character class \"" + cls + "\" specified", pc.Line, pc.Column, pc.Position, pc.FileOrUrl);
							result.AddRange(ranges);
							readFirstChar = false;
							wantRange = false;
							firstRead = false;
							continue;
						}
					}
					if (!readFirstChar)
					{
						if (char.IsHighSurrogate((char)pc.Current))
						{
							var chh = (char)pc.Current;
							pc.Advance();
							pc.Expecting();
							firstChar = char.ConvertToUtf32(chh, (char)pc.Current);
							pc.Advance();
							pc.Expecting();
						}
						else if ('\\' == pc.Current)
						{
							pc.Advance();
							firstChar = _ParseRangeEscapePart(pc);
						}
						else
						{
							firstChar = pc.Current;
							pc.Advance();
							pc.Expecting();
						}
						readFirstChar = true;

					}
					else
					{
						if ('-' == pc.Current)
						{
							pc.Advance();
							pc.Expecting();
							wantRange = true;
						}
						else
						{
							result.Add(firstChar);
							result.Add(firstChar);
							readFirstChar = false;
						}
					}
					firstRead = false;
				}
				else
				{
					if ('\\' != pc.Current)
					{
						var ch = 0;
						if (char.IsHighSurrogate((char)pc.Current))
						{
							var chh = (char)pc.Current;
							pc.Advance();
							pc.Expecting();
							ch = char.ConvertToUtf32(chh, (char)pc.Current);
						}
						else
							ch = (char)pc.Current;
						pc.Advance();
						pc.Expecting();
						result.Add(firstChar);
						result.Add(ch);
					}
					else
					{
						result.Add(firstChar);
						pc.Advance();
						result.Add(_ParseRangeEscapePart(pc));
					}
					wantRange = false;
					readFirstChar = false;
				}

			}
			if (readFirstChar)
			{
				result.Add(firstChar);
				result.Add(firstChar);
				if (wantRange)
				{
					result.Add('-');
					result.Add('-');
				}
			}
			pc.Expecting(']');
			pc.Advance();
			return new KeyValuePair<bool, int[]>(isNot, result.ToArray());
		}
		static FFA _ParseModifier(FFA expr, LexContext pc, int accept)
		{
			var line = pc.Line;
			var column = pc.Column;
			var position = pc.Position;
			switch (pc.Current)
			{
				case '*':
					expr = Repeat(expr, 0, 0, accept);
					pc.Advance();
					break;
				case '+':
					expr = Repeat(expr, 1, 0, accept);
					pc.Advance();
					break;
				case '?':
					expr = Optional(expr, accept);
					pc.Advance();
					break;
				case '{':
					pc.Advance();
					pc.TrySkipWhiteSpace();
					pc.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '}');
					var min = -1;
					var max = -1;
					if (',' != pc.Current && '}' != pc.Current)
					{
						var l = pc.CaptureBuffer.Length;
						pc.TryReadDigits();
						min = int.Parse(pc.GetCapture(l));
						pc.TrySkipWhiteSpace();
					}
					if (',' == pc.Current)
					{
						pc.Advance();
						pc.TrySkipWhiteSpace();
						pc.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '}');
						if ('}' != pc.Current)
						{
							var l = pc.CaptureBuffer.Length;
							pc.TryReadDigits();
							max = int.Parse(pc.GetCapture(l));
							pc.TrySkipWhiteSpace();
						}
					}
					else { max = min; }
					pc.Expecting('}');
					pc.Advance();
					expr = Repeat(expr, min, max, accept);
					break;
			}
			return expr;
		}
		static byte _FromHexChar(char hex)
		{
			if (':' > hex && '/' < hex)
				return (byte)(hex - '0');
			if ('G' > hex && '@' < hex)
				return (byte)(hex - '7'); // 'A'-10
			if ('g' > hex && '`' < hex)
				return (byte)(hex - 'W'); // 'a'-10
			throw new ArgumentException("The value was not hex.", "hex");
		}
		static bool _IsHexChar(char hex)
		{
			if (':' > hex && '/' < hex)
				return true;
			if ('G' > hex && '@' < hex)
				return true;
			if ('g' > hex && '`' < hex)
				return true;
			return false;
		}
		// return type is either char or ranges. this is kind of a union return type.
		static int _ParseEscapePart(LexContext pc)
		{
			if (-1 == pc.Current) return -1;
			switch (pc.Current)
			{
				case 'f':
					pc.Advance();
					return '\f';
				case 'v':
					pc.Advance();
					return '\v';
				case 't':
					pc.Advance();
					return '\t';
				case 'n':
					pc.Advance();
					return '\n';
				case 'r':
					pc.Advance();
					return '\r';
				case 'x':
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return 'x';
					byte b = _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					return unchecked((char)b);
				case 'u':
					if (-1 == pc.Advance())
						return 'u';
					ushort u = _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					return unchecked((char)u);
				default:
					int i = pc.Current;
					pc.Advance();
					if (char.IsHighSurrogate((char)i))
					{
						i = char.ConvertToUtf32((char)i, (char)pc.Current);
						pc.Advance();
					}
					return (char)i;
			}
		}
		static int _ParseRangeEscapePart(LexContext pc)
		{
			if (-1 == pc.Current)
				return -1;
			switch (pc.Current)
			{
				case '0':
					pc.Advance();
					return '\0';
				case 'f':
					pc.Advance();
					return '\f';
				case 'v':
					pc.Advance();
					return '\v';
				case 't':
					pc.Advance();
					return '\t';
				case 'n':
					pc.Advance();
					return '\n';
				case 'r':
					pc.Advance();
					return '\r';
				case 'x':
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return 'x';
					byte b = _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar((char)pc.Current))
						return unchecked((char)b);
					b <<= 4;
					b |= _FromHexChar((char)pc.Current);
					return unchecked((char)b);
				case 'u':
					if (-1 == pc.Advance())
						return 'u';
					ushort u = _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked((char)u);
					u |= _FromHexChar((char)pc.Current);
					return unchecked((char)u);
				default:
					int i = pc.Current;
					pc.Advance();
					if (char.IsHighSurrogate((char)i))
					{
						i = char.ConvertToUtf32((char)i, (char)pc.Current);
						pc.Advance();
					}
					return (char)i;
			}
		}
		static KeyValuePair<int, int>[] _ToPairs(int[] packedRanges)
		{
			var result = new KeyValuePair<int, int>[packedRanges.Length / 2];
			for (var i = 0; i < result.Length; ++i)
			{
				var j = i * 2;
				result[i] = new KeyValuePair<int, int>(packedRanges[j], packedRanges[j + 1]);
			}
			return result;
		}
		static int[] _FromPairs(IList<KeyValuePair<int, int>> pairs)
		{
			var result = new int[pairs.Count * 2];
			for (int ic = pairs.Count, i = 0; i < ic; ++i)
			{
				var pair = pairs[i];
				var j = i * 2;
				result[j] = pair.Key;
				result[j + 1] = pair.Value;
			}
			return result;
		}
		static IList<KeyValuePair<int, int>> _NotRanges(int[] ranges)
		{
			return new List<KeyValuePair<int, int>>(_NotRanges(_ToPairs(ranges)));
		}
		static IEnumerable<KeyValuePair<int, int>> _NotRanges(IEnumerable<KeyValuePair<int, int>> ranges)
		{
			// expects ranges to be normalized
			var last = 0x10ffff;
			using (var e = ranges.GetEnumerator())
			{
				if (!e.MoveNext())
				{
					yield return new KeyValuePair<int, int>(0x0, 0x10ffff);
					yield break;
				}
				if (e.Current.Key > 0)
				{
					yield return new KeyValuePair<int, int>(0, unchecked(e.Current.Key - 1));
					last = e.Current.Value;
					if (0x10ffff <= last)
						yield break;
				} else if(e.Current.Key==0) {
					last = e.Current.Value;
					if (0x10ffff <= last)
						yield break;
				}
				while (e.MoveNext())
				{
					if (0x10ffff <= last)
						yield break;
					if (unchecked(last + 1) < e.Current.Key)
						yield return new KeyValuePair<int, int>(unchecked(last + 1), unchecked((e.Current.Key - 1)));
					last = e.Current.Value;
				}
				if (0x10ffff > last)
					yield return new KeyValuePair<int, int>(unchecked((last + 1)), 0x10ffff);

			}

		}
		/// <summary>
		/// Makes the machine into a DFA
		/// </summary>
		/// <returns>A new machine that is the DFA equivalent of this machine</returns>
		public FFA ToDfa()
		{
			return _Determinize(this);
		}
		/// <summary>
		/// Makes a minimized version of this machine
		/// </summary>
		/// <returns>A machine with spurious states eliminated</returns>
		public FFA ToMinimized()
		{
			return _Minimize(this);
		}
		private void _Totalize()
		{
			_Totalize(FillClosure());
		}
		static void _Totalize(IList<FFA> closure)
		{
			var s = new FFA();
			s.Transitions.Add(new FFATransition(0, 0x10ffff, s));
			foreach (FFA p in closure)
			{
				int maxi = 0;
				var sortedTrans = new List<FFATransition>(p.Transitions);
				sortedTrans.Sort((x, y) => { var c = x.Min.CompareTo(y.Min); if (0 != c) return c; return x.Max.CompareTo(y.Max); });
				foreach (var t in sortedTrans)
				{
					if (t.Min > maxi)
					{
						p.Transitions.Add(new FFATransition(maxi, (t.Min - 1), s));
					}

					if (t.Max + 1 > maxi)
					{
						maxi = t.Max + 1;
					}
				}

				if (maxi <= 0x10ffff)
				{
					p.Transitions.Add(new FFATransition(maxi, 0x10ffff, s));
				}
			}
		}

		static FFA _Minimize(FFA a)
		{
			a = a.ToDfa();
			var tr = a.Transitions;
			if (1 == tr.Count)
			{
				FFATransition t = tr[0];
				if (t.To == a && t.Min == 0 && t.Max == 0x10ffff)
				{
					return a;
				}
			}

			a._Totalize();

			// Make arrays for numbered states and effective alphabet.
			var cl = a.FillClosure();
			var states = new FFA[cl.Count];
			int number = 0;
			foreach (var q in cl)
			{
				states[number] = q;
				q.Tag = number;
				++number;
			}

			var pp = new List<int>();
			for (int ic = cl.Count, i = 0; i < ic; ++i)
			{
				var ffa = cl[i];
				pp.Add(0);
				foreach (var t in ffa.Transitions)
				{
					pp.Add(t.Min);
					if (t.Max < 0x10ffff)
					{
						pp.Add((t.Max + 1));
					}
				}
			}

			var sigma = new int[pp.Count];
			pp.CopyTo(sigma, 0);
			Array.Sort(sigma);

			// Initialize data structures.
			var reverse = new List<List<Queue<FFA>>>();
			foreach (var s in states)
			{
				var v = new List<Queue<FFA>>();
				_Init(v, sigma.Length);
				reverse.Add(v);
			}

			var reverseNonempty = new bool[states.Length, sigma.Length];

			var partition = new List<LinkedList<FFA>>();
			_Init(partition, states.Length);

			var block = new int[states.Length];
			var active = new _FList[states.Length, sigma.Length];
			var active2 = new _FListNode[states.Length, sigma.Length];
			var pending = new Queue<_IntPair>();
			var pending2 = new bool[sigma.Length, states.Length];
			var split = new List<FFA>();
			var split2 = new bool[states.Length];
			var refine = new List<int>();
			var refine2 = new bool[states.Length];

			var splitblock = new List<List<FFA>>();
			_Init(splitblock, states.Length);

			for (int q = 0; q < states.Length; q++)
			{
				splitblock[q] = new List<FFA>();
				partition[q] = new LinkedList<FFA>();
				for (int x = 0; x < sigma.Length; x++)
				{
					reverse[q][x] = new Queue<FFA>();
					active[q, x] = new _FList();
				}
			}

			// Find initial partition and reverse edges.
			foreach (var qq in states)
			{
				int j = qq.IsAccepting ? 0 : 1;

				partition[j].AddLast(qq);
				block[qq.Tag] = j;
				for (int x = 0; x < sigma.Length; x++)
				{
					var y = sigma[x];
					var p = qq._Step(y);
					var pn = p.Tag;
					reverse[pn][x].Enqueue(qq);
					reverseNonempty[pn, x] = true;
				}
			}

			// Initialize active sets.
			for (int j = 0; j <= 1; j++)
			{
				for (int x = 0; x < sigma.Length; x++)
				{
					foreach (var qq in partition[j])
					{
						if (reverseNonempty[qq.Tag, x])
						{
							active2[qq.Tag, x] = active[j, x].Add(qq);
						}
					}
				}
			}

			// Initialize pending.
			for (int x = 0; x < sigma.Length; x++)
			{
				int a0 = active[0, x].Count;
				int a1 = active[1, x].Count;
				int j = a0 <= a1 ? 0 : 1;
				pending.Enqueue(new _IntPair(j, x));
				pending2[x, j] = true;
			}

			// Process pending until fixed point.
			int k = 2;
			while (pending.Count > 0)
			{
				_IntPair ip = pending.Dequeue();
				int p = ip.N1;
				int x = ip.N2;
				pending2[x, p] = false;

				// Find states that need to be split off their blocks.
				for (var m = active[p, x].First; m != null; m = m.Next)
				{
					foreach (var s in reverse[m.State.Tag][x])
					{
						if (!split2[s.Tag])
						{
							split2[s.Tag] = true;
							split.Add(s);
							int j = block[s.Tag];
							splitblock[j].Add(s);
							if (!refine2[j])
							{
								refine2[j] = true;
								refine.Add(j);
							}
						}
					}
				}

				// Refine blocks.
				foreach (int j in refine)
				{
					if (splitblock[j].Count < partition[j].Count)
					{
						LinkedList<FFA> b1 = partition[j];
						LinkedList<FFA> b2 = partition[k];
						foreach (var s in splitblock[j])
						{
							b1.Remove(s);
							b2.AddLast(s);
							block[s.Tag] = k;
							for (int c = 0; c < sigma.Length; c++)
							{
								_FListNode sn = active2[s.Tag, c];
								if (sn != null && sn.StateList == active[j, c])
								{
									sn.Remove();
									active2[s.Tag, c] = active[k, c].Add(s);
								}
							}
						}

						// Update pending.
						for (int c = 0; c < sigma.Length; c++)
						{
							int aj = active[j, c].Count;
							int ak = active[k, c].Count;
							if (!pending2[c, j] && 0 < aj && aj <= ak)
							{
								pending2[c, j] = true;
								pending.Enqueue(new _IntPair(j, c));
							}
							else
							{
								pending2[c, k] = true;
								pending.Enqueue(new _IntPair(k, c));
							}
						}

						k++;
					}

					foreach (var s in splitblock[j])
					{
						split2[s.Tag] = false;
					}

					refine2[j] = false;
					splitblock[j].Clear();
				}

				split.Clear();
				refine.Clear();
			}

			// Make a new state for each equivalence class, set initial state.
			var newstates = new FFA[k];
			for (int n = 0; n < newstates.Length; n++)
			{
				var s = new FFA();
				newstates[n] = s;
				foreach (var q in partition[n])
				{
					if (q == a)
					{
						a = s;
					}

					s.IsAccepting = q.IsAccepting;
					s.AcceptSymbol = q.AcceptSymbol;
					s.Tag = q.Tag; // Select representative.
					q.Tag = n;
				}
			}

			// Build transitions and set acceptance.
			foreach (var s in newstates)
			{
				var st = states[s.Tag];
				s.IsAccepting = st.IsAccepting;
				s.AcceptSymbol = st.AcceptSymbol;
				foreach (var t in st.Transitions)
				{
					s.Transitions.Add(new FFATransition(t.Min, t.Max, newstates[t.To.Tag]));
				}
			}
			// remove dead transitions
			foreach (var ffa in a.FillClosure())
			{
				var itrns = new List<FFATransition>(ffa.Transitions);
				foreach (var trns in itrns)
				{
					var acc = trns.To.FillAcceptingStates();
					if (0 == acc.Count)
					{
						ffa.Transitions.Remove(trns);
					}
				}
			}
			return a;
		}
		FFA _Step(int input)
		{
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				if (t.Min <= input && input <= t.Max)
					return t.To;

			}
			return null;
		}
		static void _Init<T>(IList<T> list, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add(default(T));
			}
		}
		private sealed class _IntPair
		{
			private readonly int n1;
			private readonly int n2;

			public _IntPair(int n1, int n2)
			{
				this.n1 = n1;
				this.n2 = n2;
			}

			public int N1 {
				get { return n1; }
			}

			public int N2 {
				get { return n2; }
			}
		}
		private sealed class _FList
		{
			public int Count { get; set; }

			public _FListNode First { get; set; }

			public _FListNode Last { get; set; }

			public _FListNode Add(FFA q)
			{
				return new _FListNode(q, this);
			}
		}
		/// <summary>
		/// Creates a packed DFA table from this machine
		/// </summary>
		/// <returns>A new array that contains this machine as a DFA table</returns>
		public int[] ToDfaTable() {
			var working = new List<int>();
			var closure = new List<F.FFA>();
			var fa = this;
			fa.FillClosure(closure);
			var isDfa = true;
			foreach(var cfa in closure) {
				if(!cfa.IsDeterministic) {
					isDfa = false;
					break;
                }
            }
			if(!isDfa) {
				fa = fa.ToDfa();
				closure.Clear();
				fa.FillClosure(closure);
			}
			var stateIndices = new int[closure.Count];
			for (var i = 0; i < closure.Count; ++i) {
				var cfa = closure[i];
				stateIndices[i] = working.Count;
				// add the accept
				working.Add(cfa.IsAccepting ? cfa.AcceptSymbol : -1);
				var itrgp = cfa.FillInputTransitionRangesGroupedByState();
				// add the number of transitions
				working.Add(itrgp.Count);
				foreach (var itr in itrgp) {
					// We have to fill in the following after the fact
					// We don't have enough info here
					// for now just drop the state index as a placeholder
					working.Add(closure.IndexOf(itr.Key));
					// add the number of packed ranges
					working.Add(itr.Value.Length / 2);
					// add the packed ranges
					working.AddRange(itr.Value);

				}
			}
			var result = working.ToArray();
			var state = 0;
			while (state < result.Length) {
				state++;
				var tlen = result[state++];
				for (var i = 0; i < tlen; ++i) {
					// patch the destination
					result[state] = stateIndices[result[state]];
					++state;
					var prlen = result[state++];
					state += prlen * 2;
				}
			}
			return result;
		}
		/// <summary>
		/// Creates a machine based on the given DFA table
		/// </summary>
		/// <param name="dfa">The DFA table</param>
		/// <returns>A new machine based on the DFA table</returns>
		public static FFA FromDfaTable(int[] dfa) {
			if (null == dfa) return null;
			if(dfa.Length == 0) return new FFA();
			var si = 0;
			var states = new Dictionary<int, FFA>();
			while (si<dfa.Length) {
				var fa = new FFA();
				states.Add(si, fa);
				fa.AcceptSymbol = dfa[si++];
				if (fa.AcceptSymbol != -1) fa.IsAccepting = true;
				var tlen = dfa[si++];
				for(var i = 0;i<tlen;++i) {
					++si; // tto
					var prlen = dfa[si++];
					si += prlen * 2;
                }
			}
			si = 0;
			var sid = 0;
			while (si < dfa.Length) {
				var fa = states[si];
				var acc = dfa[si++];
				var tlen = dfa[si++];
				for (var i = 0; i < tlen; ++i) {
					var tto = dfa[si++];
					var to = states[tto];
					var prlen = dfa[si++];
					for(var j=0;j<prlen;++j) {
						var pmin = dfa[si++];
						var pmax = dfa[si++];
						fa.Transitions.Add(new FFATransition(pmin, pmax, to));
					}
				}
				++sid;
			}
			return states[0];
		}

		private sealed class _FListNode
		{
			public _FListNode(FFA q, _FList sl)
			{
				State = q;
				StateList = sl;
				if (sl.Count++ == 0)
				{
					sl.First = sl.Last = this;
				}
				else
				{
					sl.Last.Next = this;
					Prev = sl.Last;
					sl.Last = this;
				}
			}

			public _FListNode Next { get; private set; }

			private _FListNode Prev { get; set; }

			public _FList StateList { get; private set; }

			public FFA State { get; private set; }

			public void Remove()
			{
				StateList.Count--;
				if (StateList.First == this)
				{
					StateList.First = Next;
				}
				else
				{
					Prev.Next = Next;
				}

				if (StateList.Last == this)
				{
					StateList.Last = Prev;
				}
				else
				{
					Next.Prev = Prev;
				}
			}
		}

		static FFA _Determinize(FFA fa)
		{
			var p = new HashSet<int>();
			var closure = new List<FFA>();
			fa.FillClosure(closure);
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var ffa = closure[i];
				p.Add(0);
				foreach (var t in ffa.Transitions)
				{
					p.Add(t.Min);
					if (t.Max < 0x10ffff)
					{
						p.Add((t.Max + 1));
					}
				}
			}

			var points = new int[p.Count];
			p.CopyTo(points, 0);
			Array.Sort(points);

			var sets = new Dictionary<KeySet<FFA>, KeySet<FFA>>();
			var working = new Queue<KeySet<FFA>>();
			var dfaMap = new Dictionary<KeySet<FFA>, FFA>();
			var initial = new KeySet<FFA>();
			initial.Add(fa);
			sets.Add(initial, initial);
			working.Enqueue(initial);
			var result = new FFA();
			foreach (var afa in initial)
			{
				if (afa.IsAccepting)
				{
					result.IsAccepting = true;
					result.AcceptSymbol = afa.AcceptSymbol;
					break;
				}
			}
			dfaMap.Add(initial, result);
			while (working.Count > 0)
			{
				var s = working.Dequeue();
				FFA dfa;
				dfaMap.TryGetValue(s, out dfa);
				foreach (FFA q in s)
				{
					if (q.IsAccepting)
					{
						dfa.IsAccepting = true;
						dfa.AcceptSymbol = q.AcceptSymbol;
						break;
					}
				}

				for (var i = 0; i < points.Length; i++)
				{
					var pnt = points[i];
					var set = new KeySet<FFA>();
					foreach (FFA c in s)
					{
						foreach (var trns in c.Transitions)
						{
							if (trns.Min <= pnt && pnt <= trns.Max)
							{
								set.Add(trns.To);
							}
						}
					}
					if (!sets.ContainsKey(set))
					{
						sets.Add(set, set);
						working.Enqueue(set);
						dfaMap.Add(set, new FFA());
					}

					FFA dst;
					dfaMap.TryGetValue(set, out dst);
					int first = pnt;
					int last;
					if (i + 1 < points.Length)
						last = (points[i + 1] - 1);
					else
						last = 0x10ffff;
					dfa.Transitions.Add(new FFATransition(first, last, dst));
				}

			}
			// remove dead transitions
			foreach (var ffa in result.FillClosure())
			{
				var itrns = new List<FFATransition>(ffa.Transitions);
				foreach (var trns in itrns)
				{
					var acc = trns.To.FillAcceptingStates();
					if (0 == acc.Count)
					{
						ffa.Transitions.Remove(trns);
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Escapes a single codepoint
		/// </summary>
		/// <param name="codepoint">The codepoint</param>
		/// <param name="builder">The optional <see cref="StringBuilder"/> to write to.</param>
		/// <returns>The escaped codepoint</returns>
		public static string EscapeCodepoint(int codepoint,StringBuilder builder = null) {
			if(null==builder)
				builder = new StringBuilder();
			switch (codepoint) {
			case '.':
			case '[':
			case ']':
			case '^':
			case '-':
			case '\\':
				builder.Append('\\');
				builder.Append(char.ConvertFromUtf32(codepoint));
				break;
			case '\t':
				builder.Append("\\t");
				break;
			case '\n':
				builder.Append("\\n");
				break;
			case '\r':
				builder.Append("\\r");
				break;
			case '\0':
				builder.Append("\\0");
				break;
			case '\f':
				builder.Append("\\f");
				break;
			case '\v':
				builder.Append("\\v");
				break;
			case '\b':
				builder.Append("\\b");
				break;
			default:
				var s = char.ConvertFromUtf32(codepoint);
				if (!char.IsLetterOrDigit(s, 0) && !char.IsSeparator(s, 0) && !char.IsPunctuation(s, 0) && !char.IsSymbol(s, 0)) {
					if (s.Length == 1) {
						builder.Append("\\u");
						builder.Append(unchecked((ushort)codepoint).ToString("x4"));
					} else {
						builder.Append("\\U");
						builder.Append(codepoint.ToString("x8"));
					}

				} else
					builder.Append(s);
				break;
			}
			return builder.ToString();
		}
		private sealed class _EFA {
			public bool IsAccepting;
			public int Accept;
			public List<KeyValuePair<StringBuilder, _EFA>> Transitions { get; } = new List<KeyValuePair<StringBuilder, _EFA>>();
			public IList<_EFA> FillClosure(IList<_EFA> result = null) {
				if (result == null) result = new List<_EFA>();
				if (result.Contains(this))
					return result;
				result.Add(this);
				foreach (var t in Transitions) {
					t.Value.FillClosure(result);
				}

				return result;
			}
			public static IList<KeyValuePair<_EFA, string>> GetIncomingTransitions(IEnumerable<_EFA> closure, _EFA efa, bool includeLoops = true) {
				var result = new List<KeyValuePair<_EFA, string>>();
				foreach (var cfa in closure) {
					foreach (var t in cfa.Transitions) {
						if (includeLoops || t.Value != cfa) {
							if (t.Value == efa) {
								result.Add(new KeyValuePair<_EFA, string>(cfa, t.Key.ToString()));
							}
						}
					}
				}
				return result;
			}
			public IDictionary<_EFA, StringBuilder> FillInputTransitionsGroupedByState(IDictionary<_EFA, StringBuilder> result = null) {
				if (result == null) {
					result = new Dictionary<_EFA, StringBuilder>();
				}
				var h = new HashSet<_EFA>();
				for (var i = 0; i < Transitions.Count; ++i) {
					var t = Transitions[i];
					StringBuilder exp;
					if (!result.TryGetValue(t.Value, out exp)) {
						var sb = new StringBuilder(t.Key.ToString());
						result.Add(t.Value, sb);
					} else {
						if (!h.Add(t.Value)) {
							exp.Remove(exp.Length - 1, 1);
						} else {
							exp.Insert(0, "(");
						}
						exp.Append("|");
						exp.Append(t.Key.ToString());
						exp.Append(")");
					}
				}
				return result;
			}
			public int IndexOfTransition(string value) {
				var result = 0;
				foreach (var t in Transitions) {
					if (t.ToString().Equals(value, StringComparison.InvariantCulture)) {
						return result;
					}
					++result;
				}
				return -1;
			}
		}
		static void _AppendRangeTo(StringBuilder builder, int[] ranges, int index) {
			var first = ranges[index];
			var last = ranges[index + 1];
			EscapeCodepoint(first,builder);
			if (0 == last.CompareTo(first)) return;
			if (last == first + 1) // spit out 1 length ranges as two chars
			{
				EscapeCodepoint(last,builder);
				return;
			}
			builder.Append('-');
			EscapeCodepoint(last,builder);
		}
		/// <summary>
		/// Returns a regular expression that will match the same pattern as this machine.
		/// </summary>
		/// <returns>A regular expression that is equivilent to this machine</returns>
		public override string ToString() {
			// Still somewhat untested
			var closure = FillClosure();
			IList<_EFA> efas = new List<_EFA>(closure.Count + 1);
			var i = 0;
			while (i <= closure.Count) {
				efas.Add(null);
				++i;
			}
			i = 0;
			foreach (var cfa in closure) {
				efas[i] = new _EFA();
				++i;
			}
			var final = new _EFA();
			final.IsAccepting = true;
			final.Accept = 0;
			efas[i] = final;
			for (i = 0; i < closure.Count; ++i) {
				var e = efas[i];
				var c = closure[i];
				if (c.IsAccepting) {
					e.Transitions.Add(new KeyValuePair<StringBuilder, _EFA>(new StringBuilder(), final));
				}
				var rngGrps = c.FillInputTransitionRangesGroupedByState();
				foreach (var rngGrp in rngGrps) {
					var tto = efas[closure.IndexOf(rngGrp.Key)];
					var sb = new StringBuilder();
					IList<KeyValuePair<int, int>> rngs = _ToPairs(rngGrp.Value);
					var nrngs = new List<KeyValuePair<int, int>>(_NotRanges(rngs));
					var isNot = false;
					if (nrngs.Count < rngs.Count || (nrngs.Count == rngs.Count && 0x10ffff == rngs[rngs.Count - 1].Value)) {
						isNot = true;
						if (0 != nrngs.Count) {
							sb.Append("^");
						} else {
							sb.Append(".");
						}
						rngs = nrngs;
					}
					var rpairs = _FromPairs(rngs);
					for (var r = 0; r < rpairs.Length; r += 2) {
						_AppendRangeTo(sb, rpairs, r);
					}
					if (isNot || sb.Length != 1 || (char.IsWhiteSpace(sb.ToString(), 0))) {
						sb.Insert(0, '[');
						sb.Append(']');
					}
					e.Transitions.Add(new KeyValuePair<StringBuilder, _EFA>(sb, tto));
				}
			}
			i = 0;
			var done = false;
			while (!done) {
				done = true;
				var innerDone = false;
				while (!innerDone) {
					innerDone = true;
					foreach (var e in efas) {
						if (e.Transitions.Count == 1) {
							var its = _EFA.GetIncomingTransitions(efas, e);
							if (its.Count == 1 && its[0].Key.Transitions.Count == 1) {
								// is a loop?
								if (e.Transitions[0].Value == its[0].Key) {
									if (e.Transitions[0].Key.Length == 1) {
										e.Transitions[0].Key.Append("*");
									} else {
										e.Transitions[0].Key.Insert(0, "(");
										e.Transitions[0].Key.Append(")*");
									}
								} else {
									its[0].Key.Transitions[0] = new KeyValuePair<StringBuilder, _EFA>(its[0].Key.Transitions[0].Key, e.Transitions[0].Value);
									its[0].Key.Transitions[0].Key.Append(e.Transitions[0].Key.ToString());
								}
								innerDone = false;
								efas = efas[0].FillClosure();
							} else {
								foreach (var it in its) {
									// is it a loop?
									if (efas.IndexOf(it.Key) >= efas.IndexOf(e)) {
										// yes
									} else {
										// no
										for (var ii = 0; ii < it.Key.Transitions.Count; ++ii) {
											if (it.Value == it.Key.Transitions[ii].Key.ToString()) {
												it.Key.Transitions[ii] = new KeyValuePair<StringBuilder, _EFA>(it.Key.Transitions[ii].Key, e.Transitions[0].Value);
												it.Key.Transitions[ii].Key.Append(e.Transitions[0].Key.ToString());
												innerDone = false;
												efas = efas[0].FillClosure();
											}
										}
									}
								}
							}
						}
					}
					if (innerDone) {
						efas = efas[0].FillClosure();
					} else
						done = false;
					// combine the unions
					innerDone = false;
					while (!innerDone) {
						innerDone = true;
						foreach (var e in efas) {
							var rgs = e.FillInputTransitionsGroupedByState();
							if (rgs.Count != e.Transitions.Count) {
								e.Transitions.Clear();
								foreach (var rg in rgs) {
									e.Transitions.Add(new KeyValuePair<StringBuilder, _EFA>(rg.Value, rg.Key));
								}
								innerDone = false;
								efas = efas[0].FillClosure();
							}
						}
					}
					if (innerDone) {
						efas = efas[0].FillClosure();
					} else
						done = false;

					// remove the loops
					innerDone = false;
					while (!innerDone) {
						innerDone = true;
						foreach (var e in efas) {
							for (var ii = 0; ii < e.Transitions.Count; ++ii) {
								var t = e.Transitions[ii];
								if (t.Value == e) {
									// this is a loop
									if (t.Key.Length > 1) {
										t.Key.Insert(0, "(");
										t.Key.Append(")");
									}
									t.Key.Append("*");
									// prepend it to all the other transitions 
									for (var iii = 0; iii < e.Transitions.Count; ++iii) {
										if (ii != iii) {
											var tt = e.Transitions[iii];
											if (tt.Value != e) {
												tt.Key.Insert(0, t.Key.ToString());

											}
										}
									}
									e.Transitions.RemoveAt(ii);
									--ii;
									innerDone = false;
									efas = efas[0].FillClosure();
								}

							}
						}
					}
					if (innerDone) {
						efas = efas[0].FillClosure();
					} else
						done = false;
				}
			}
			return efas[0].Transitions[0].Key.ToString();
		}
		/// <summary>
		/// Retrieves a transition index given a specified UTF32 codepoint
		/// </summary>
		/// <param name="codepoint">The codepoint</param>
		/// <returns>The index of the matching transition or a negative number if no match was found.</returns>
		public int FindTransitionIndex(int codepoint) {
			for(var i = 0;i<Transitions.Count;++i) {
				var t = Transitions[i];
				if (t.Min > codepoint) { 
					return -1;
                }
				if(t.Max>=codepoint) {
					return i;
                }
            }
			return -1;
        }
		/// <summary>
		/// Indicates whether or not the collection of states contains an accepting state
		/// </summary>
		/// <param name="states">The states to check</param>
		/// <returns>True if one or more of the states is accepting, otherwise false</returns>
		public static bool HasAcceptingState(IEnumerable<FFA> states) {
			foreach(var state in states) {
				if (state.IsAccepting) return true;
            }
			return false;
		}
		/// <summary>
		/// Fills a list with all of the new states after moving from a given set of states along a given input. (NFA-move)
		/// </summary>
		/// <param name="states">The current states</param>
		/// <param name="codepoint">The codepoint to move on</param>
		/// <param name="result">A list to hold the next states. If null, one will be created.</param>
		/// <returns>The list of next states</returns>
		public static IList<FFA> FillMove(IEnumerable<FFA> states, int codepoint, IList<FFA> result = null) {
			if (result == null) result = new List<FFA>();
			foreach(var state in states) {
				var i = state.FindTransitionIndex(codepoint);
				if(-1<i) {
					var tto = state.Transitions[i].To;
					if(!result.Contains(tto)) {
						result.Add(tto);
                    }
                }
            }
			return result;
        }
		/// <summary>
		/// Returns the next state
		/// </summary>
		/// <param name="codepoint">The codepoint to move on</param>
		/// <returns>The next state, or null if there was no valid move.</returns>
		/// <remarks>This machine must be a DFA or this won't work correctly.</remarks>
		public FFA DfaMove(int codepoint) {
			var i = FindTransitionIndex(codepoint);
			if (-1 < i) {
				return Transitions[i].To;
			}
			return null;
		}
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="text">The text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public bool IsMatch(IEnumerable<char> text) {
			return IsMatch(LexContext.Create(text));
        }
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="lc">A <see cref="LexContext"/> containing the text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public bool IsMatch(LexContext lc) {
			lc.EnsureStarted();
			if(IsDeterministic) {
				var state = this;
				while(true) {
					var next = state.DfaMove(lc.Current);
					if(null==next) {
						if (state.IsAccepting) {
							return lc.Current == LexContext.EndOfInput;
						}
						return false;
                    }
					lc.Advance();
					state = next;
					if(lc.Current == LexContext.EndOfInput) {
						return state.IsAccepting;
                    }
                }
			} else {
				IList<FFA> states = new List<FFA>();
				states.Add(this);
				while(true) {
					var next = FFA.FillMove(states, lc.Current);
					if(next.Count==0) {
						if(HasAcceptingState(states)) {
							return lc.Current == LexContext.EndOfInput;
                        }
						return false;
                    }
					lc.Advance();
					states = next;
					if (lc.Current == LexContext.EndOfInput) {
						return HasAcceptingState(states);
					}
				}
            }
        }
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="text">The text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public static bool IsMatch(int[] dfa, IEnumerable<char> text) {
			return IsMatch(dfa, LexContext.Create(text));
		}

		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="lc">A <see cref="LexContext"/> containing the text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public static bool IsMatch(int[] dfa, LexContext lc) {
			lc.EnsureStarted();
			int si = 0;
			while (true) {
				// retrieve the accept id
				var acc = dfa[si++];
				if (lc.Current == LexContext.EndOfInput) {
					return acc != -1;
				}
				// get the transitions count
				var trns = dfa[si++];
				var matched = false;
				for (var i = 0; i < trns; ++i) {
					// get the destination state
					var tto = dfa[si++];
					// get the packed range count
					var prlen = dfa[si++];
					for (var j = 0; j < prlen; ++j) {
						// get the min cp
						var min = dfa[si++];
						// get the max cp
						var max = dfa[si++];
						if (min > lc.Current) {
							si += (prlen - (j+1)) * 2;
							break;
						}
						if (max >= lc.Current) {
							si = tto;
							matched = true;
							// break out of both loops
							goto next_state;
						}
					}
				}
			next_state:
				if (!matched) {
					// is the state accepting?
					if (acc != -1) {
						return lc.Current == LexContext.EndOfInput;
					}
					return false;
				}
				lc.Advance();
				if (lc.Current == LexContext.EndOfInput) {
					// is the current state accepting
					return dfa[si] != -1;
				}
			}
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="lc">The <see cref="LexContext"/> with the text to search</param>
		/// <returns>The 0 based position of the next match or less than 0 if not found.</returns>
		public long Search(LexContext lc) {
			lc.EnsureStarted();
			lc.ClearCapture();
			var result = lc.Position;
			if (IsDeterministic) {
				while (lc.Current != LexContext.EndOfInput) {
					var state = this;
					while (true) {
						var next = state.DfaMove(lc.Current);
						if (null == next) {
							if (state.IsAccepting && lc.CaptureBuffer.Length > 1) {
								return result - 1;
							}
							lc.ClearCapture();
							lc.Advance();
							result = -1;
							break;
						}
						if (result == -1) {
							result = lc.Position;
						}
						lc.Capture();
						lc.Advance();
						state = next;
						if (lc.Current == LexContext.EndOfInput) {
							return state.IsAccepting ? result - 1 : -1;
						}
					}
				}
				return this.IsAccepting ? result - 1 : -1;
			} else {
				while (lc.Current != LexContext.EndOfInput) {
					IList<FFA> states = new List<FFA>();
					states.Add(this);
					while (true) {
						var next = FFA.FillMove(states, lc.Current);
						if (next.Count == 0) {
							if (HasAcceptingState(states) && lc.CaptureBuffer.Length > 1) {
								return result - 1;
							}
							lc.ClearCapture();
							lc.Advance();
							result = -1;
							break;
						}
						if (result == -1) {
							result = lc.Position;
						}
						lc.Capture();
						lc.Advance();
						states = next;
						if (lc.Current == LexContext.EndOfInput) {
							return HasAcceptingState(states) ? result - 1 : -1;
						}
					}
				}
				return this.IsAccepting ? result - 1 : -1;
			}
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by the indicated machine
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="lc">The <see cref="LexContext"/> with the text to search</param>
		/// <returns>The 0 based position of the next match or less than 0 if not found.</returns>
		public static long Search(int[] dfa, LexContext lc) {
			lc.EnsureStarted();
			lc.ClearCapture();
			var result = lc.Position;
			while (lc.Current != LexContext.EndOfInput) {
				var si = 0;
				while (true) {
					var acc = dfa[si++];
					if (lc.Current == LexContext.EndOfInput) {
						if(acc!=-1 && lc.CaptureBuffer.Length > 1) {
							return result - 1;
                        }
					}
					var trns = dfa[si++];
					var matched = false;
					for (var i = 0; i < trns; ++i) {
						var tto = dfa[si++];
						var prlen = dfa[si++];
						for (var j = 0; j < prlen; ++j) {
							var min = dfa[si++];
							var max = dfa[si++];
							if (min > lc.Current) {
								si += (prlen - (j+1)) * 2;
								break;
							}
							if (max >= lc.Current) {
								si = tto;
								matched = true;
								goto next_state;
							}
						}
					}
				next_state:
					if (!matched) {
						if (acc != -1 && lc.CaptureBuffer.Length> 1) {
							return result - 1;
						}
						lc.ClearCapture();
						lc.Advance();
						result = -1;
						si = 0;
						break;
					}
					if (result == -1) {
						result = lc.Position;
					}
					lc.Capture();
					lc.Advance();
					if (lc.Current == LexContext.EndOfInput) {
						return dfa[si]!=-1 ? result - 1 : -1;
					}
				}
			}
			return dfa[0]!=-1 ? result - 1 : -1;
		}
	}
}
