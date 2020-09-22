using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPL20ArchiveBuilder
{

	public class Program
	{

		public static async Task Main()
		{

			using SqlConnection sqlConnection = new SqlConnection(Settings.DatabaseConnectionString);
			sqlConnection.Open();

			var blobServiceClient = new BlobServiceClient(Settings.StorageConnectionString);
			var blobContainerClient = blobServiceClient.GetBlobContainerClient("cpl20");

			var logFileLocation = @"D:\Temp\CPL20Uploads.txt";
			var alreadyUploadedVideos = new List<string>();
			if (File.Exists(logFileLocation))
			{
				string line;
				using StreamReader file = new StreamReader(logFileLocation);
				while ((line = file.ReadLine()) != null)
					if (int.TryParse(line, out var uploadedSessionId))
						alreadyUploadedVideos.Add(line);
			}

			var speakers = Speakers.GetSpeakersForEvent(10, sqlConnection);
			var sessions = Session.GetSessionForEvent(10, sqlConnection, speakers);
			var sessionTags = SessionTags.GetTags(sqlConnection);

			sqlConnection.Close();

			Console.Clear();
			Console.WriteLine("Building session pages...");

			var rootDirectory = @"C:\Code PaLOUsa 2020 Videos\";
			var pagesPath = @"D:\Repros\TaleLearnCode\CPL20ArchiveSite\src\CPL20Archive\Pages\";
			var wwwRootPath = @"D:\Repros\TaleLearnCode\CPL20ArchiveSite\src\CPL20Archive\wwwroot\";
			foreach (string sessionPeriodPath in Directory.GetDirectories(rootDirectory))
			{
				foreach (string sessionPath in Directory.GetDirectories(sessionPeriodPath))
				{
					var sessionPathComponents = sessionPath.Split('\\');
					if (sessions.ContainsKey(Convert.ToInt32(sessionPathComponents[sessionPathComponents.Length - 1])))
					{
						var session = sessions[Convert.ToInt32(sessionPathComponents[sessionPathComponents.Length - 1])];
						Console.WriteLine($"Uploading MP4 for Session {session.Id}");
						//if (!alreadyUploadedVideos.Contains(session.Id.ToString()) && (session.Id != 1779 || session.Id != 1721))
						//{
						//	await UploadVideoAsync(session.Id.ToString(), sessionPath, blobContainerClient);
						//	alreadyUploadedVideos.Add(session.Id.ToString());
						//}
						if (session.Id != 1779 || session.Id != 1721)
							sessions[session.Id].VideoUploaded = true;
						var path = $@"{pagesPath}sessions\{session.Id}\";
						Directory.CreateDirectory(path);
						Console.WriteLine($"Writing session pages for Session {session.Id}");

						var cshtmlPath = @$"{pagesPath}Sessions\{session.Id}\";
						Directory.CreateDirectory(cshtmlPath);
						File.WriteAllText($"{cshtmlPath}Index.cshtml", BuildIndexPage(session, sessionTags));
						File.WriteAllText($"{cshtmlPath}Index.cshtml.cs", BuildIndexCSFile(session.Id));

						var sessionEmbedPath = @$"{wwwRootPath}sessions\{session.Id}\";
						Directory.CreateDirectory(sessionEmbedPath);
						File.WriteAllText($"{sessionEmbedPath}player.html", BuildPlayerPage(session));
						File.WriteAllText($"{sessionEmbedPath}config.xml", BuildConfigXML(session));
						File.WriteAllText($"{sessionEmbedPath}config_xml.js", BuildConfigXMLJs(session));
						Console.WriteLine();
					}
				}
			}

			BuildSchedulePages(sessions.Values.ToList(), pagesPath);
			BuildTagPages(sessions.Values.ToList(), sessionTags, pagesPath);
			BuildTopicPages(sessions.Values.ToList(), pagesPath);

			File.WriteAllLines(logFileLocation, alreadyUploadedVideos.ToArray());

			Console.WriteLine("Done");






		}

		private static string BuildIndexPage(Session session, Dictionary<int, (string Name, string NormalizedName)> tags)
		{
			var indexPage = new StringBuilder();
			indexPage.AppendLine("@page");
			indexPage.AppendLine($"@model CPL20Archive.Pages.Sessions._{session.Id}.IndexModel");
			indexPage.AppendLine("@{");
			indexPage.AppendLine("}");
			indexPage.AppendLine("    <div class=\"top-title-area bg-img-charcoal-eticket\">");
			indexPage.AppendLine("      <div class=\"container\">");
			indexPage.AppendLine("        <h1 class=\"title-page\">Session Details</h1>");
			indexPage.AppendLine("      </div>");
			indexPage.AppendLine("    </div>");
			indexPage.AppendLine("    <div class=\"gap\"></div>");
			indexPage.AppendLine("    <div class=\"container\">");
			indexPage.AppendLine("      <div class=\"row\">");
			indexPage.AppendLine("        <div class=\"span3\">");
			indexPage.AppendLine("          <aside class=\"sidebar-left\">");
			indexPage.AppendLine("            <br />");
			indexPage.AppendLine("            <h5 class=\"SessionDetails\">Topic(s)</h5>");
			indexPage.AppendLine($"           {session.Topics.GetAsideLinks()}");
			indexPage.AppendLine("            <br />");
			indexPage.AppendLine("            <h5 class=\"SessionDetails\">Tags</h5>");
			indexPage.AppendLine($"           {session.Tags.GetAsideLinks(tags)}");
			indexPage.AppendLine("            <br />");
			indexPage.AppendLine("          </aside>");
			indexPage.AppendLine("        </div>");
			indexPage.AppendLine("        <div class=\"span9\">");
			indexPage.AppendLine($"          <h2 id=\"MainContent_MainContent_SessionTitle\" class=\"SessionDetails\">{session.Title}</h2>");
			indexPage.AppendLine($"          <h5 id=\"MainContent_MainContent_SessionType\" class=\"SessionDetails\">{session.SessionType}</h5>");
			indexPage.AppendLine("          <br />");
			indexPage.AppendLine("          <div class=\"smart-player-embed-container\">");
			indexPage.AppendLine($"            <iframe class=\"smart-player-embed-iframe\" id=\"embeddedSmartPlayerInstance\" src=\"~/sessions/{session.Id}/player.html\" scrolling=\"no\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine($"          {session.Abstract}");
			indexPage.AppendLine("          <hr />");
			indexPage.AppendLine("          <div class=\"row row-wrap\">");
			indexPage.AppendLine("            <div class=\"span3\">");
			foreach (var speaker in session.SessionSpeakers)
			{
				indexPage.AppendLine("              <div class=\"thumb center\">");
				indexPage.AppendLine("                <div class=\"thumb-header\">");
				indexPage.AppendLine($"                  <a class=\"hover-img\" href=\"http://codepalousa.com/SpeakerDetails/{speaker.Id}\">");
				indexPage.AppendLine($"                    <img src=\"https://greeneventstechnology.azureedge.net/cpl20/speakers/{speaker.FirstName}_{speaker.LastName}.png\" alt=\"{speaker.FirstName} {speaker.LastName}\" title=\"{speaker.FirstName} {speaker.LastName}\" />");
				indexPage.AppendLine("                  </a>");
				indexPage.AppendLine("                </div>");
				indexPage.AppendLine("                <div class=\"thumb-caption\">");
				indexPage.AppendLine($"                  <h5 class=\"thumb-title\"><a href=\"http://codepalousa.com/SpeakerDetails/{speaker.Id}\">{speaker.FirstName} {speaker.LastName}</a></h5>");
				indexPage.AppendLine("                  <p class=\"thumb-meta\"><br /></p>");
				indexPage.AppendLine("                </div>");
				indexPage.AppendLine("              </div>");
			}
			indexPage.AppendLine("            </div>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("        </div>");
			indexPage.AppendLine("      </div>");
			indexPage.AppendLine("      <div class=\"gap\"></div>");
			indexPage.AppendLine("    </div>");
			return indexPage.ToString();
		}

		private static string BuildIndexCSFile(int sessionId)
		{
			var pageModel = new StringBuilder();
			pageModel.AppendLine("using System;");
			pageModel.AppendLine("using System.Collections.Generic;");
			pageModel.AppendLine("using System.Linq;");
			pageModel.AppendLine("using System.Threading.Tasks;");
			pageModel.AppendLine("using Microsoft.AspNetCore.Mvc;");
			pageModel.AppendLine("using Microsoft.AspNetCore.Mvc.RazorPages;");
			pageModel.AppendLine("");
			pageModel.AppendLine($"namespace CPL20Archive.Pages.Sessions._{sessionId}");
			pageModel.AppendLine("{");
			pageModel.AppendLine("    public class IndexModel : PageModel");
			pageModel.AppendLine("    {");
			pageModel.AppendLine("        public void OnGet()");
			pageModel.AppendLine("        {");
			pageModel.AppendLine("        }");
			pageModel.AppendLine("    }");
			pageModel.AppendLine("}");
			return pageModel.ToString();
		}

		private static string BuildPlayerPage(Session session)
		{
			var playerPage = new StringBuilder();
			playerPage.AppendLine("<!DOCTYPE html>");
			playerPage.AppendLine("<html>");
			playerPage.AppendLine("<head>");
			playerPage.AppendLine("  <meta name=\"google\" value=\"notranslate\" />");
			playerPage.AppendLine("  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			playerPage.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
			playerPage.AppendLine("  <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\">");
			playerPage.AppendLine("  <title></title>");
			playerPage.AppendLine("  <link href=\"https://fonts.googleapis.com/css?family=Quicksand|Actor|Source+Sans+Pro:900|Lato:400,700,900|Oswald:400,700|Abel:400|Dosis:600\" rel=\"stylesheet\">");
			playerPage.AppendLine("  <link href=\"https://greeneventstechnology.azureedge.net/cpl20/css/techsmith-smart-player.min.css\" rel=\"stylesheet\" type=\"text/css\" />");
			playerPage.AppendLine("");
			playerPage.AppendLine("  <style>");
			playerPage.AppendLine("    html, body {");
			playerPage.AppendLine("      background-color: #1a1a1a;");
			playerPage.AppendLine("    }");
			playerPage.AppendLine("  </style>");
			playerPage.AppendLine("</head>");
			playerPage.AppendLine("<body>");
			playerPage.AppendLine("");
			playerPage.AppendLine("  <div id=\"tscVideoContent\">");
			playerPage.AppendLine("    <img width=\"32px\" height=\"32px\" style=\"position: absolute; top: 50%; left: 50%; margin: -16px 0 0 -16px\"");
			playerPage.AppendLine("         src=\"data:image/gif;base64,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\">");
			playerPage.AppendLine("  </div>");
			playerPage.AppendLine("");
			playerPage.AppendLine("  <script src=\"config_xml.js\"></script>");
			playerPage.AppendLine("  <script type=\"text/javascript\">");
			playerPage.AppendLine("    (function (window) {");
			playerPage.AppendLine("      function setup(TSC) {");
			playerPage.AppendLine($"        TSC.playerConfiguration.addMediaSrc(\"https://greeneventstechnology.azureedge.net/cpl20/videos/{session.Id}.mp4\");");
			playerPage.AppendLine("        TSC.playerConfiguration.setXMPSrc(\"config.xml\");");
			playerPage.AppendLine("        TSC.playerConfiguration.setAutoHideControls(true);");
			playerPage.AppendLine("        TSC.playerConfiguration.setBackgroundColor(\"#000000\");");
			playerPage.AppendLine("        TSC.playerConfiguration.setCaptionsEnabled(false);");
			playerPage.AppendLine("        TSC.playerConfiguration.setSidebarEnabled(false);");
			playerPage.AppendLine("");
			playerPage.AppendLine("        TSC.playerConfiguration.setAutoPlayMedia(false);");
			playerPage.AppendLine($"        TSC.playerConfiguration.setPosterImageSrc(\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\");");
			playerPage.AppendLine("        TSC.playerConfiguration.setIsSearchable(true);");
			playerPage.AppendLine("        TSC.playerConfiguration.setEndActionType(\"stop\");");
			playerPage.AppendLine("        TSC.playerConfiguration.setEndActionParam(\"true\");");
			playerPage.AppendLine("        TSC.playerConfiguration.setAllowRewind(-1);");
			playerPage.AppendLine("");
			playerPage.AppendLine("");
			playerPage.AppendLine("        TSC.localizationStrings.setLanguage(TSC.languageCodes.ENGLISH);");
			playerPage.AppendLine("");
			playerPage.AppendLine("        TSC.mediaPlayer.init(\"#tscVideoContent\");");
			playerPage.AppendLine("      }");
			playerPage.AppendLine("");
			playerPage.AppendLine("      function loadScript(e, t) { if (!e || !(typeof e === \"string\")) { return } var n = document.createElement(\"script\"); if (typeof document.attachEvent === \"object\") { n.onreadystatechange = function () { if (n.readyState === \"complete\" || n.readyState === \"loaded\") { if (t) { t() } } } } else { n.onload = function () { if (t) { t() } } } n.src = e; document.getElementsByTagName(\"head\")[0].appendChild(n) }");
			playerPage.AppendLine("");
			playerPage.AppendLine("      loadScript('https://greeneventstechnology.azureedge.net/cpl20/js/techsmith-smart-player.min.js', function () {");
			playerPage.AppendLine("        setup(window[\"TSC\"]);");
			playerPage.AppendLine("      });");
			playerPage.AppendLine("    }(window));");
			playerPage.AppendLine("  </script>");
			playerPage.AppendLine("</body>");
			playerPage.AppendLine("</html>");
			return playerPage.ToString();
		}

		private static string BuildConfigXML(Session session)
		{
			var configXML = new StringBuilder();
			configXML.AppendLine("<x:xmpmeta tsc:version=\"2.0.1\" xmlns:x=\"adobe:ns:meta/\" xmlns:tsc=\"http://www.techsmith.com/xmp/tsc/\">");
			configXML.AppendLine("   <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\" xmlns:xmpG=\"http://ns.adobe.com/xap/1.0/g/\" xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" xmlns:tscDM=\"http://www.techsmith.com/xmp/tscDM/\" xmlns:tscIQ=\"http://www.techsmith.com/xmp/tscIQ/\" xmlns:tscHS=\"http://www.techsmith.com/xmp/tscHS/\" xmlns:stDim=\"http://ns.adobe.com/xap/1.0/sType/Dimensions#\" xmlns:stFnt=\"http://ns.adobe.com/xap/1.0/sType/Font#\" xmlns:exif=\"http://ns.adobe.com/exif/1.0\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");
			configXML.AppendLine($"      <rdf:Description dc:date=\"{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss tt}\" dc:source=\"Camtasia,20.0.7,enu\" dc:title=\"{session.Id}\" tscDM:firstFrame=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\" tscDM:originId=\"F02DFD84-CD76-4FF1-B6E7-7636DA8D96AA\" tscDM:project=\"{session.Id}\">");
			configXML.AppendLine("         <xmpDM:duration xmpDM:scale=\"1/1000\" xmpDM:value=\"2413716\"/>");
			configXML.AppendLine("         <xmpDM:videoFrameSize stDim:unit=\"pixel\" stDim:h=\"720\" stDim:w=\"1280\"/>");
			configXML.AppendLine("         <tsc:langName>");
			configXML.AppendLine("            <rdf:Bag>");
			configXML.AppendLine("               <rdf:li xml:lang=\"en-US\">English</rdf:li></rdf:Bag>");
			configXML.AppendLine("         </tsc:langName>");
			configXML.AppendLine("         <xmpDM:Tracks>");
			configXML.AppendLine("            <rdf:Bag>");
			configXML.AppendLine("            </rdf:Bag>");
			configXML.AppendLine("         </xmpDM:Tracks>");
			configXML.AppendLine("         <tscDM:controller>");
			configXML.AppendLine("            <rdf:Description xmpDM:name=\"tscplayer\">");
			configXML.AppendLine("               <tscDM:parameters>");
			configXML.AppendLine("                  <rdf:Bag>");
			configXML.AppendLine("                     <rdf:li xmpDM:name=\"autohide\" xmpDM:value=\"true\"/><rdf:li xmpDM:name=\"autoplay\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"loop\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"searchable\" xmpDM:value=\"true\"/><rdf:li xmpDM:name=\"captionsenabled\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"sidebarenabled\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"unicodeenabled\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"backgroundcolor\" xmpDM:value=\"000000\"/><rdf:li xmpDM:name=\"sidebarlocation\" xmpDM:value=\"left\"/><rdf:li xmpDM:name=\"endaction\" xmpDM:value=\"stop\"/><rdf:li xmpDM:name=\"endactionparam\" xmpDM:value=\"true\"/><rdf:li xmpDM:name=\"locale\" xmpDM:value=\"en-US\"/></rdf:Bag>");
			configXML.AppendLine("               </tscDM:parameters>");
			configXML.AppendLine("               <tscDM:controllerText>");
			configXML.AppendLine("                  <rdf:Bag>");
			configXML.AppendLine("                  </rdf:Bag>");
			configXML.AppendLine("               </tscDM:controllerText>");
			configXML.AppendLine("            </rdf:Description>");
			configXML.AppendLine("         </tscDM:controller>");
			configXML.AppendLine("         <tscDM:contentList>");
			configXML.AppendLine("            <rdf:Description>");
			configXML.AppendLine("               <tscDM:files>");
			configXML.AppendLine("                  <rdf:Seq>");
			configXML.AppendLine($"                     <rdf:li xmpDM:name=\"0\" xmpDM:value=\"{session.Id}.mp4\"/><rdf:li xmpDM:name=\"1\" xmpDM:value=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\"/><rdf:li xmpDM:name=\"2\" xmpDM:value=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\"/></rdf:Seq>");
			configXML.AppendLine("               </tscDM:files>");
			configXML.AppendLine("            </rdf:Description>");
			configXML.AppendLine("         </tscDM:contentList>");
			configXML.AppendLine("      </rdf:Description>");
			configXML.AppendLine("   </rdf:RDF>");
			configXML.AppendLine("</x:xmpmeta>");
			return configXML.ToString();
		}

		private static string BuildConfigXMLJs(Session session)
		{
			var js = new StringBuilder();
			js.AppendLine("var TSC = TSC || {};");
			js.AppendLine("");
			js.AppendLine("TSC.embedded_config_xml = '<x:xmpmeta tsc:version=\"2.0.1\" xmlns:x=\"adobe:ns:meta/\" xmlns:tsc=\"http://www.techsmith.com/xmp/tsc/\">\\");
			js.AppendLine("   <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\" xmlns:xmpG=\"http://ns.adobe.com/xap/1.0/g/\" xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" xmlns:tscDM=\"http://www.techsmith.com/xmp/tscDM/\" xmlns:tscIQ=\"http://www.techsmith.com/xmp/tscIQ/\" xmlns:tscHS=\"http://www.techsmith.com/xmp/tscHS/\" xmlns:stDim=\"http://ns.adobe.com/xap/1.0/sType/Dimensions#\" xmlns:stFnt=\"http://ns.adobe.com/xap/1.0/sType/Font#\" xmlns:exif=\"http://ns.adobe.com/exif/1.0\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\\");
			js.AppendLine($"      <rdf:Description dc:date=\"2020-09-03 10:27:14 PM\" dc:source=\"Camtasia,20.0.7,enu\" dc:title=\"1658\" tscDM:firstFrame=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\" tscDM:originId=\"F02DFD84-CD76-4FF1-B6E7-7636DA8D96AA\" tscDM:project=\"{session.Id}\">\\");
			js.AppendLine("         <xmpDM:duration xmpDM:scale=\"1/1000\" xmpDM:value=\"2413716\"/>\\");
			js.AppendLine("         <xmpDM:videoFrameSize stDim:unit=\"pixel\" stDim:h=\"720\" stDim:w=\"1280\"/>\\");
			js.AppendLine("         <tsc:langName>\\");
			js.AppendLine("            <rdf:Bag>\\");
			js.AppendLine("               <rdf:li xml:lang=\"en-US\">English</rdf:li></rdf:Bag>\\");
			js.AppendLine("         </tsc:langName>\\");
			js.AppendLine("         <xmpDM:Tracks>\\");
			js.AppendLine("            <rdf:Bag>\\");
			js.AppendLine("            </rdf:Bag>\\");
			js.AppendLine("         </xmpDM:Tracks>\\");
			js.AppendLine("         <tscDM:controller>\\");
			js.AppendLine("            <rdf:Description xmpDM:name=\"tscplayer\">\\");
			js.AppendLine("               <tscDM:parameters>\\");
			js.AppendLine("                  <rdf:Bag>\\");
			js.AppendLine("                     <rdf:li xmpDM:name=\"autohide\" xmpDM:value=\"true\"/><rdf:li xmpDM:name=\"autoplay\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"loop\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"searchable\" xmpDM:value=\"true\"/><rdf:li xmpDM:name=\"captionsenabled\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"sidebarenabled\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"unicodeenabled\" xmpDM:value=\"false\"/><rdf:li xmpDM:name=\"backgroundcolor\" xmpDM:value=\"000000\"/><rdf:li xmpDM:name=\"sidebarlocation\" xmpDM:value=\"left\"/><rdf:li xmpDM:name=\"endaction\" xmpDM:value=\"stop\"/><rdf:li xmpDM:name=\"endactionparam\" xmpDM:value=\"true\"/><rdf:li xmpDM:name=\"locale\" xmpDM:value=\"en-US\"/></rdf:Bag>\\");
			js.AppendLine("               </tscDM:parameters>\\");
			js.AppendLine("               <tscDM:controllerText>\\");
			js.AppendLine("                  <rdf:Bag>\\");
			js.AppendLine("                  </rdf:Bag>\\");
			js.AppendLine("               </tscDM:controllerText>\\");
			js.AppendLine("            </rdf:Description>\\");
			js.AppendLine("         </tscDM:controller>\\");
			js.AppendLine("         <tscDM:contentList>\\");
			js.AppendLine("            <rdf:Description>\\");
			js.AppendLine("               <tscDM:files>\\");
			js.AppendLine("                  <rdf:Seq>\\");
			js.AppendLine($"                     <rdf:li xmpDM:name=\"0\" xmpDM:value=\"1658.mp4\"/><rdf:li xmpDM:name=\"1\" xmpDM:value=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\"/><rdf:li xmpDM:name=\"2\" xmpDM:value=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\"/></rdf:Seq>\\");
			js.AppendLine("               </tscDM:files>\\");
			js.AppendLine("            </rdf:Description>\\");
			js.AppendLine("         </tscDM:contentList>\\");
			js.AppendLine("      </rdf:Description>\\");
			js.AppendLine("   </rdf:RDF>\\");
			js.AppendLine("</x:xmpmeta>';");
			return js.ToString();
		}

		private static string BuildListingHeader((string Name, string NormalizedName) currentPage, List<(string Name, string NormalizedName)> items, string pageTitle)
		{
			var header = new StringBuilder();
			header.AppendLine("@page");
			header.AppendLine($"@model CPL20Archive.Pages.Sessions.{currentPage.NormalizedName}Model");
			header.AppendLine("@{");
			header.AppendLine("}");
			header.AppendLine("<div class=\"top-title-area bg-img-charcoal-eticket\">");
			header.AppendLine("  <div class=\"container\">");
			header.AppendLine($"    <h1 class=\"title-page\">{pageTitle}</h1>");
			header.AppendLine("  </div>");
			header.AppendLine("</div>");
			header.AppendLine("<div class=\"gap\"></div>");
			header.AppendLine("<div class=\"container\">");
			header.AppendLine("  <div class=\"demo-buttons\">");
			foreach (var item in items)
				header.AppendLine(BuildTabLink(item.NormalizedName, item.Name, currentPage));
			header.AppendLine("  </div>");
			header.AppendLine("  <div class=\"gap\"></div>");
			return header.ToString();
		}

		private static void BuildSchedulePages(List<Session> sessions, string pagesPath)
		{

			var sessionPeriods = new Dictionary<int, (string Name, string NormalizedName)>
			{
				{ 105, ("Workshops", "Workshops") },
				{ 108, ("Period 1", "Period1") },
				{ 109, ("Period 2", "Period2") },
				{ 110, ("Period 3", "Period3") },
				{ 111, ("Period 4", "Period4") },
				{ 112, ("Period 5", "Period5") },
				{ 113, ("Period 6", "Period6") },
				{ 114, ("Period 7", "Period7") },
				{ 115, ("Period 8", "Period8") },
				{ 116, ("Period 9", "Period9") },
				{ 117, ("Period 10", "Period10") },
				{ 118, ("Period 11", "Period11") },
				{ 119, ("Keynote", "Keynote") }
			};

			var sessionPages = new Dictionary<int, StringBuilder>();
			foreach (var sessionPeriod in sessionPeriods)
				sessionPages.Add(sessionPeriod.Key, new StringBuilder(BuildListingHeader(sessionPeriod.Value, sessionPeriods.Values.ToList(), "Sessions by Period")));

			foreach (var session in sessions)
			{
				if (session.SessionPeriodId != 121)
				{
					var sessionListing = new StringBuilder();
					sessionListing.AppendLine("  <div class=\"row\">");
					sessionListing.AppendLine("    <div class=\"span4\">");
					sessionListing.AppendLine($"      <a asp-page=\"/Sessions/{session.Id}/Index\">");
					sessionListing.AppendLine($"        <img style=\"width: 320px; height: 180px\" src=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\" />");
					sessionListing.AppendLine("       </a>");
					sessionListing.AppendLine("    </div>");
					sessionListing.AppendLine("    <div class=\"span8\">");
					sessionListing.AppendLine($"      <a asp-page=\"/Sessions/{session.Id}/Index\">");
					sessionListing.AppendLine($"      <h3>{session.Title}</h3>");
					sessionListing.AppendLine("       </a>");
					if (session.Id == 1779 || session.Id == 1721)
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#DF625C;foreground-color:#FFFFFF;\">&nbsp;Video Not Available&nbsp;</span></p>");
					else if (session.VideoUploaded)
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#A3CF63;\">&nbsp;Video Available&nbsp;</span></p>");
					else
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#DF625C;foreground-color:#FFFFFF;\">&nbsp;Video Not Available Yet&nbsp;</span></p>");
					sessionListing.AppendLine("    </div>");
					sessionListing.AppendLine("  </div>");
					sessionListing.AppendLine("  <hr />");
					if (session.SessionPeriodId == 106 || session.SessionPeriodId == 107 || session.SessionPeriodId == 120)
						sessionPages[105].Append(sessionListing.ToString());
					else
						sessionPages[session.SessionPeriodId].Append(sessionListing.ToString());
				}
			}

			var path = $@"{pagesPath}Sessions\";
			foreach (var sessionPeriod in sessionPeriods)
			{
				File.WriteAllText($"{path}{sessionPeriod.Value.NormalizedName}.cshtml", sessionPages[sessionPeriod.Key].ToString());
				File.WriteAllText($"{path}{sessionPeriod.Value.NormalizedName}.cshtml.cs", GetCSFile("Sessions", sessionPeriod.Value.NormalizedName));
			}

		}

		private static string BuildTabLink(string pageFileName, string pageName, (string Name, string NormalizedName) selectedTag)
		{
			if (pageName == selectedTag.Name)
				return $"    <a asp-page=\"{pageFileName}\" class=\"btn btn-info\">{pageName}</a>";
			else
				return $"    <a asp-page=\"{pageFileName}\" class=\"btn\">{pageName}</a>";
		}

		private static void BuildTagPages(List<Session> sessions, Dictionary<int, (string Name, string NormalizedName)> sessionTags, string pagesPath)
		{
			var tagPages = new Dictionary<int, StringBuilder>();
			foreach (var sessionTag in sessionTags)
				tagPages.Add(sessionTag.Key, new StringBuilder(BuildListingHeader(sessionTag.Value, sessionTags.Values.ToList(), "Sessions by Tag")));

			foreach (var session in sessions)
			{
				foreach (var tag in session.Tags)
				{
					var sessionListing = new StringBuilder();
					sessionListing.AppendLine("  <div class=\"row\">");
					sessionListing.AppendLine("    <div class=\"span4\">");
					sessionListing.AppendLine($"      <a asp-page=\"/Sessions/{session.Id}/Index\">");
					sessionListing.AppendLine($"        <img style=\"width: 320px; height: 180px\" src=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\" />");
					sessionListing.AppendLine("       </a>");
					sessionListing.AppendLine("    </div>");
					sessionListing.AppendLine("    <div class=\"span8\">");
					sessionListing.AppendLine($"      <a asp-page=\"/Sessions/{session.Id}/Index\">");
					sessionListing.AppendLine($"      <h3>{session.Title}</h3>");
					sessionListing.AppendLine("       </a>");
					if (session.Id == 1779 || session.Id == 1721)
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#DF625C;foreground-color:#FFFFFF;\">&nbsp;Video Not Available&nbsp;</span></p>");
					else if (session.VideoUploaded)
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#A3CF63;\">&nbsp;Video Available&nbsp;</span></p>");
					else
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#DF625C;foreground-color:#FFFFFF;\">&nbsp;Video Not Available Yet&nbsp;</span></p>");
					sessionListing.AppendLine("    </div>");
					sessionListing.AppendLine("  </div>");
					sessionListing.AppendLine("  <hr />");
					tagPages[tag].Append(sessionListing.ToString());
				}

			}

			var path = $@"{pagesPath}Sessions\";
			foreach (var tag in sessionTags)
			{
				File.WriteAllText($"{path}{tag.Value.NormalizedName}.cshtml", tagPages[tag.Key].ToString());
				File.WriteAllText($"{path}{tag.Value.NormalizedName}.cshtml.cs", GetCSFile("Sessions", tag.Value.NormalizedName));
			}

		}

		private static void BuildTopicPages(List<Session> sessions, string pagesPath)
		{

			var topics = new Dictionary<int, (string Name, string NormalizedName)>()
			{
				{ 1, ("Application Development", "Topic_AppDev") },
				{ 2, ("Infrastructure", "Topic_Infrastructure") },
				{ 3, ("Project Mangement", "Topic_ProjMgnt") },
				{ 4, ("Requirements", "Topic_Requirements") },
				{ 5, ("Soft Skills", "Topic_SoftSkills") },
				{ 6, ("Software Testing", "Topic_Testing") },
				{ 7, ("User Experience", "Topic_UX") }
			};

			var topicPages = new Dictionary<int, StringBuilder>();
			foreach (var topic in topics)
				topicPages.Add(topic.Key, new StringBuilder(BuildListingHeader(topic.Value, topics.Values.ToList(), "Sessions by Topic")));

			foreach (var session in sessions)
			{
				foreach (var topic in session.Topics)
				{
					var sessionListing = new StringBuilder();
					sessionListing.AppendLine("  <div class=\"row\">");
					sessionListing.AppendLine("    <div class=\"span4\">");
					sessionListing.AppendLine($"      <a asp-page=\"/Sessions/{session.Id}/Index\">");
					sessionListing.AppendLine($"        <img style=\"width: 320px; height: 180px\" src=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.Id}.jpg\" />");
					sessionListing.AppendLine("       </a>");
					sessionListing.AppendLine("    </div>");
					sessionListing.AppendLine("    <div class=\"span8\">");
					sessionListing.AppendLine($"      <a asp-page=\"/Sessions/{session.Id}/Index\">");
					sessionListing.AppendLine($"      <h3>{session.Title}</h3>");
					sessionListing.AppendLine("       </a>");
					if (session.Id == 1779 || session.Id == 1721)
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#DF625C;foreground-color:#FFFFFF;\">&nbsp;Video Not Available&nbsp;</span></p>");
					else if (session.VideoUploaded)
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#A3CF63;\">&nbsp;Video Available&nbsp;</span></p>");
					else
						sessionListing.AppendLine($"      <p>{session.Summary}...<br /><br /><span style=\"background-color:#DF625C;foreground-color:#FFFFFF;\">&nbsp;Video Not Available Yet&nbsp;</span></p>");
					sessionListing.AppendLine("    </div>");
					sessionListing.AppendLine("  </div>");
					sessionListing.AppendLine("  <hr />");
					topicPages[topic].Append(sessionListing.ToString());
				}

			}

			var path = $@"{pagesPath}Sessions\";
			foreach (var topic in topics)
			{
				File.WriteAllText($"{path}{topic.Value.NormalizedName}.cshtml", topicPages[topic.Key].ToString());
				File.WriteAllText($"{path}{topic.Value.NormalizedName}.cshtml.cs", GetCSFile("Sessions", topic.Value.NormalizedName));
			}

		}

		private static string GetCSFile(string pageFolder, string pageName)
		{
			var csFile = new StringBuilder();
			csFile.AppendLine("using Microsoft.AspNetCore.Mvc.RazorPages;");
			csFile.AppendLine($"namespace CPL20Archive.Pages.{pageFolder}");
			csFile.AppendLine("{");
			csFile.AppendLine($"	public class {pageName}Model : PageModel");
			csFile.AppendLine("	{");
			csFile.AppendLine("		public void OnGet()");
			csFile.AppendLine("		{");
			csFile.AppendLine("		}");
			csFile.AppendLine("	}");
			csFile.AppendLine("}");
			return csFile.ToString();
		}

		private static Dictionary<int, string> GetSessionPeriods()
		{
			return new Dictionary<int, string>()
			{
				{ 105, "Workshops" },
				{ 108, "Session Period 1" },
				{ 109, "Session Period 2" },
				{ 110, "Session Period 3" },
				{ 111, "Session Period 4" },
				{ 112, "Session Period 5" },
				{ 113, "Session Period 6" },
				{ 114, "Session Period 7" },
				{ 115, "Session Period 8" },
				{ 116, "Session Period 9" },
				{ 117, "Session Period 10" },
				{ 118, "Session Period 11" },
				{ 119, "Keynote" }
			};
		}

		public static async Task UploadVideoAsync(string sessionId, string filePath, BlobContainerClient container)
		{
			BlobClient blob = container.GetBlobClient($"videos/{sessionId}.mp4");
			using FileStream uploadFileStream = File.OpenRead($@"{filePath}\{sessionId}.mp4");
			await blob.UploadAsync(uploadFileStream, new BlobHttpHeaders { ContentType = "video/mp4" }, conditions: null).ConfigureAwait(true);
		}

	}

}