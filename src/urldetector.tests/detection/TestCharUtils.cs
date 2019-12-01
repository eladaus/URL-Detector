using System.Collections.Generic;
using System.Text.RegularExpressions;
using urldetector.detection;
using Xunit;

namespace urldetector.tests.detection
{
	public class TestCharUtils
	{

		public static IEnumerable<object[]> GetSplitStrings =>
			new List<object[]>
			{
				new object[] {"192.168.1.1"},
				new object[] {".."},
				new object[] {"192%2e168%2e1%2e1"},
				new object[] {"asdf"},
				new object[] {"192.39%2e1%2E1"},
				new object[] {"as\uFF61awe.a3r23.lkajsf0ijr...."},
				new object[] {"%2e%2easdf"},
				new object[] {"sdoijf%2e"},
				new object[] {"ksjdfh.asdfkj.we%2"},
				new object[] {"0xc0%2e0x00%2e0x02%2e0xeb"},
				new object[] {""}
			};


		[Theory] 
		[MemberData(nameof(GetSplitStrings))]
		public void TestSplitByDot(string stringToSplit)
		{
			var regex = "[\\.\u3002\uFF0E\uFF61]|%2e|%2E";
			var expected = Regex.Split(stringToSplit, regex);
			Assert
				.Equal(CharUtils.SplitByDot(stringToSplit), expected);
		}

		[Fact]
		public void TestCharUtilsIsAlpha()
		{
			char[] arr = {'a', 'Z', 'f', 'X'};
			foreach (var a in arr)
			{
				Assert.True(CharUtils.IsAlpha(a));
			}

			char[] arr2 = {'0', '9', '[', '~'};
			foreach (var a in arr2)
			{
				Assert.False(CharUtils.IsAlpha(a));
			}
		}

		[Fact]
		public void TestCharUtilsIsAlphaNumeric()
		{
			char[] arr = {'a', 'G', '3', '9'};
			foreach (var a in arr)
			{
				Assert.True(CharUtils.IsAlphaNumeric(a));
			}

			char[] arr2 = {'~', '-', '_', '\n'};
			foreach (var a in arr2)
			{
				Assert.False(CharUtils.IsAlphaNumeric(a));
			}
		}

		[Fact]
		public void TestCharUtilsIsHex()
		{
			char[] arr = {'a', 'A', '0', '9'};
			foreach (var a in arr)
			{
				Assert.True(CharUtils.IsHex(a));
			}

			char[] arr2 = {'~', ';', 'Z', 'g'};
			foreach (var a in arr2)
			{
				Assert.False(CharUtils.IsHex(a));
			}
		}

		[Fact]
		public void TestCharUtilsIsNumeric()
		{
			char[] arr = {'0', '4', '6', '9'};
			foreach (var a in arr)
			{
				Assert.True(CharUtils.IsNumeric(a));
			}

			// Seems you can do this Zero thing in java but not c#
			char[] arr2 = {'a', '~', 'A' /*, 0*/};
			foreach (var a in arr2)
			{
				Assert.False(CharUtils.IsNumeric(a));
			}
		}

		[Fact]
		public void TestCharUtilsIsUnreserved()
		{
			char[] arr = {'-', '.', 'a', '9', 'Z', '_', 'f'};
			foreach (var a in arr)
			{
				Assert.True(CharUtils.IsUnreserved(a));
			}

			char[] arr2 = {' ', '!', '(', '\n'};
			foreach (var a in arr2)
			{
				Assert.False(CharUtils.IsUnreserved(a));
			}
		}
	}
}