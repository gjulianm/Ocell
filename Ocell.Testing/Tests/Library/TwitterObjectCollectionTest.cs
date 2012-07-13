using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ocell.Library;
using DanielVaughan;
using DanielVaughan.Services;
using DanielVaughan.Services.Implementation;
using System.Threading;
using Ocell.LightTwitterService;

namespace Ocell.Testing.Tests.Library
{

	[TestClass]
	[Tag("Library")]
	[Tag("String manipulation")]
	public class TwitterObjectCollectionTest
	{
		[TestMethod]
		[Description("Tests well formed structures")]
		public void TestCorrectStructures()
		{
			string[] structures = {
				"{}", "{},{}", "{{}, {}}", "{with words}", "{{with even{more}words{}{}{{{}}}}{And{More{Braces}}}}",
				"I{Should{Be{}{}{Really{Happy}}If{This}{Works}}{}{}{I dont think so, at least at first}}{It wont}",
				"{We{Should{Try{With}}}Things{}Like\\{this{}or{this\\{}}Malforming it\\} But really not"};
				
			foreach(var structure in structures)
			{
				var collection = new TwitterObjectCollection(structure);
				Assert.IsTrue(collection.IsWellFormed());
			}
		}
		
		[TestMethod]
		[Description("Tests malformed strings")]
		public void TestMalformedStructures()
		{
			string[] structures = {"{", "}", "{{}{This}}}", "Or{This{ShouldBe}", "Checked{}As{}}Bad{}Structures"};
				
			foreach(var structure in structures)
			{
				var collection = new TwitterObjectCollection(structure);
				Assert.IsFalse(collection.IsWellFormed());
			}
		}
	}
    [TestClass]
    [Tag("Library")]
	[Tag("String manipulation")]
    public class TwitterObjectEnumeratorTest
    {    
    	[TestMethod]
    	[Description("Tests a simple JSON structure")]
    	public void Simple()
    	{
    		string contents = "[";
    		string[] words = {"this", "is", "a", "test"};
    		
    		foreach(var word in words)
    			contents += "{" + word + "}";
    		
    		contents += "]";

            var enumerator = new TwitterObjectCollectionEnumerator(contents);   	
    		
    		int index = 0;
    		while(enumerator.MoveNext())
    		{
    			Assert.AreEqual('{' + words[index]+'}', enumerator.Current.ToString() );
    			index++;
    		}
    	}
    	
    	[TestMethod]
    	[Description("Tests a nested JSON structure")]
    	public void Nested()
    	{
    		string contents = "[";
    		string[] words = {"{nested}", "{}", "{Thing}", "{Happy}"};
    		
    		foreach(var word in words)
    			contents += "{" + word + "}";
    		
    		contents += "]";
    		
    		var enumerator = new TwitterObjectCollectionEnumerator(contents);   	
    		
    		int index = 0;
    		while(enumerator.MoveNext())
    		{
                Assert.AreEqual('{' + words[index] + '}', enumerator.Current.ToString());
    			index++;
    		}
    	}
    	
    	[TestMethod]
    	[Description("Tests a nested JSON structure with multiple braces")]
    	public void MultipleNested()
    	{
    		string contents = "[";
            string[] words = { "{nice}{Thing}", "{{pretty}nice}", "{{hope}{it}{works}}", "{with{multiple{levels}}}" };
    		
    		foreach(var word in words)
    			contents += "{" + word + "}";
    		
    		contents += "]";

            var enumerator = new TwitterObjectCollectionEnumerator(contents);   	
    		
    		int index = 0;
    		while(enumerator.MoveNext())
    		{
    			Assert.AreEqual('{' + words[index]+'}', enumerator.Current.ToString());
    			index++;
    		}
    	}
    }
}
