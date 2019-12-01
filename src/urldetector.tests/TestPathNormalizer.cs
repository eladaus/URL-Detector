using System.Collections.Generic;
using Xunit;

namespace urldetector.tests
{
	public class TestPathNormalizer
	{
		public static IEnumerable<object[]> GetPaths =>
			new List<object[]>
			{
				new object[] {"/%25%32%35", "/%25"},
				new object[] {"/%2%2%2", "/%252%252%252"},
				new object[] {"/%2%%335", "/%25"},
				new object[] {"/%25%32%35%25%32%35", "/%25%25"},
				new object[] {"/%2525252525252525", "/%25"},
				new object[] {"/asdf%25%32%35asd", "/asdf%25asd"},
				new object[] {"/%%%25%32%35asd%%", "/%25%25%25asd%25%25"},
				new object[] {"/%2E%73%65%63%75%72%65/%77%77%77%2E%65%62%61%79%2E%63%6F%6D/", "/.secure/www.ebay.com/"},
				new object[]
				{
					"/uploads/%20%20%20%20/.verify/.eBaysecure=updateuserdataxplimnbqmn-xplmvalidateinfoswqpcmlx=hgplmcx/",
					"/uploads/%20%20%20%20/.verify/.eBaysecure=updateuserdataxplimnbqmn-xplmvalidateinfoswqpcmlx=hgplmcx/"
				},
				new object[]
				{
					"/%257Ea%2521b%2540c%2523d%2524e%25f%255E00%252611%252A22%252833%252944_55%252B",
					"/~a!b@c%23d$e%25f^00&11*22(33)44_55+"
				},
				new object[] {"/lala/.././../..../", "/..../"},
				new object[] {"//asdfasdf/awef/sadf/sdf//", "/asdfasdf/awef/sadf/sdf/"},
				new object[] {"/", "/"},
				new object[] {"/a/../b/c", "/b/c"},
				new object[] {"/blah/..", "/"},
				new object[] {"../", "../"},
				new object[] {"/asdf/.", "/asdf/"},
				new object[] {"/a/b/./././././../c/d", "/a/c/d"},
				new object[] {"/a/b//////.///././././../c/d", "/a/c/d"},
				new object[] {"//../a/c/..///sdf", "/a/sdf"},
				new object[] {"/../asdf", "/asdf"},
				new object[] {"/../asdf/", "/asdf/"},
				new object[] {"/a/b/..c", "/a/b/..c"},
				new object[] {"/a/b/.././", "/a/"},
				new object[] {"/a/b/./", "/a/b/"},
				new object[] {"/a/b/../..", "/"},
				new object[] {"/a/b/../../../../../../", "/"},
				new object[] {"/a/b/../../../../../..", "/"},
				new object[] {"/a/b/../../../../../../c/d", "/c/d"},
				new object[] {"/a/b/../../../../../../c/d/", "/c/d/"},
				new object[] {"/a/b/../.", "/a/"},
				new object[] {"/a/b/..", "/a/"},
				new object[] {"/1.html", "/1.html"},
				new object[] {"/1/2.html?param=1", "/1/2.html?param=1"},
				new object[] {"/a./b.", "/a./b."},
				new object[] {"/a./b./", "/a./b./"}
			};

		[Theory] 
		[MemberData(nameof(GetPaths))]
		public void TestPaths(string path, string expectedPath)
		{
			var pathNormalizer = new PathNormalizer();

			Assert.Equal(pathNormalizer.NormalizePath(path), expectedPath);
		}
	}
}