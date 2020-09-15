using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CPL20ArchiveBuilder
{
	class Program
	{

		static async Task Main()
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
			var outputPath = @"D:\Repros\TaleLearnCode\CPL20ArchiveSite\src\CPL20Archive\wwwroot\";
			foreach (string sessionPeriodPath in Directory.GetDirectories(rootDirectory))
			{
				foreach (string sessionPath in Directory.GetDirectories(sessionPeriodPath))
				{
					var sessionPathComponents = sessionPath.Split('\\');
					if (sessions.ContainsKey(Convert.ToInt32(sessionPathComponents[sessionPathComponents.Length - 1])))
					{
						var session = sessions[Convert.ToInt32(sessionPathComponents[sessionPathComponents.Length - 1])];
						Console.WriteLine($"Uploading MP4 for Session {session.Id}");
						//if (!alreadyUploadedVideos.Contains(session.Id.ToString()))
						//{
						//	await UploadVideoAsync(session.Id, sessionPath, blobContainerClient);
						//	alreadyUploadedVideos.Add(session.Id);
						//}
						var path = $@"{outputPath}sessions\{session.Id}\";
						Directory.CreateDirectory(path);
						Console.WriteLine($"Writing session pages for Session {session.Id}");
						File.WriteAllText($"{path}index.html", BuildIndexPage(session, sessionTags));
						File.WriteAllText($"{path}player.html", BuildPlayerPage(session));
						File.WriteAllText($"{path}config.xml", BuildConfigXML(session));
						File.WriteAllText($"{path}config_xml.js", BuildConfigXMLJs(session));
						Console.WriteLine();
					}
				}
			}

			File.WriteAllLines(logFileLocation, alreadyUploadedVideos.ToArray());






		}

		private static string BuildIndexPage(Session session, Dictionary<int, string> tags)
		{
			var indexPage = new StringBuilder();
			indexPage.AppendLine("<!DOCTYPE html>");
			indexPage.AppendLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
			indexPage.AppendLine("<head>");
			indexPage.AppendLine("  <meta charset=\"utf-8\" />");
			indexPage.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
			indexPage.AppendLine($"  <title>Code PaLOUsa 2020 — </title>{session.Title}");
			indexPage.AppendLine("  <link href=\"https://greeneventstechnology.azureedge.net/cpl20/favicon.ico\" rel=\"shortcut icon\" type=\"image/x-icon\" />");
			indexPage.AppendLine("  <!-- meta info -->");
			indexPage.AppendLine("  <meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" />");
			indexPage.AppendLine("  <meta name=\"keywords\" content=\"Code PaLOUsa, conference, software development, software\" />");
			indexPage.AppendLine($"  <meta name=\"description\" content=\"{session.Summary}\" />");
			indexPage.AppendLine("  <meta name=\"author\" content=\"Code PaLOUsa\" />");
			indexPage.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
			indexPage.AppendLine("  <!-- Google fonts -->");
			indexPage.AppendLine("  <link href=\"http://fonts.googleapis.com/css?family=Open+Sans:400italic,400,700\" rel=\"stylesheet\" type=\"text/css\" />");
			indexPage.AppendLine("  <link href=\"http://fonts.googleapis.com/css?family=Roboto:400,300,700\" rel=\"stylesheet\" type=\"text/css\" />");
			indexPage.AppendLine("  <!-- Bootstrap styles -->");
			indexPage.AppendLine("  <link rel=\"stylesheet\" href=\"https://greeneventstechnology.azureedge.net/cpl20/css/boostrap.css\" />");
			indexPage.AppendLine("  <link rel=\"stylesheet\" href=\"https://greeneventstechnology.azureedge.net/cpl20/css/boostrap_responsive.css\" />");
			indexPage.AppendLine("  <!-- Main Template styles -->");
			indexPage.AppendLine("  <link rel=\"stylesheet\" href=\"https://greeneventstechnology.azureedge.net/cpl20/css/font_awesome.css\" />");
			indexPage.AppendLine("  <link rel=\"stylesheet\" href=\"https://greeneventstechnology.azureedge.net/cpl20/css/styles.css\" />");
			indexPage.AppendLine("  <link rel=\"stylesheet\" href=\"https://greeneventstechnology.azureedge.net/cpl20/css/mystyles.css\" />");
			indexPage.AppendLine("  <!-- Modernizr script -->");
			indexPage.AppendLine("  <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/modernizr.custom.26633.js\"></script>");
			indexPage.AppendLine("  <style type=\"text/css\">");
			indexPage.AppendLine("    h2.SessionDetails {");
			indexPage.AppendLine("      margin-bottom: 0px;");
			indexPage.AppendLine("    }");
			indexPage.AppendLine("");
			indexPage.AppendLine("    h3.SessionDetails {");
			indexPage.AppendLine("      font-size: 16.9px;");
			indexPage.AppendLine("    }");
			indexPage.AppendLine("");
			indexPage.AppendLine("    h5.SessionDetails {");
			indexPage.AppendLine("      margin-bottom: 0px;");
			indexPage.AppendLine("    }");
			indexPage.AppendLine("  </style>");
			indexPage.AppendLine("  <link href=\"https://greeneventstechnology.azureedge.net/cpl20/css/embed.css\" rel=\"stylesheet\" type=\"text/css\">");
			indexPage.AppendLine("  <meta charset=\"utf-8\" />");
			indexPage.AppendLine("</head>");
			indexPage.AppendLine("<body class=\"width1200 sticky-header\">");
			indexPage.AppendLine("  <div class=\"container-global\">");
			indexPage.AppendLine("    <header class=\"main shrink\">");
			indexPage.AppendLine("      <div class=\"container\">");
			indexPage.AppendLine("        <div class=\"row\">");
			indexPage.AppendLine("          <div class=\"span3\">");
			indexPage.AppendLine("            <a id=\"Logo\" class=\"logo\" href=\"http://localhost:55989/Default.aspx\"><img src=\"https://greeneventstechnology.azureedge.net/cpl20/img/logo/HeaderLogo.png\" alt=\"\" /></a>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("          <div class=\"span7\">");
			indexPage.AppendLine("            <div class=\"flexnav-menu-button\" id=\"flexnav-menu-button\">Menu</div>");
			indexPage.AppendLine("            <nav>");
			indexPage.AppendLine("              <ul class=\"nav nav-pills flexnav\" id=\"flexnav\" data-breakpoint=\"800\">");
			indexPage.AppendLine("                <li><a href=\"/Default.aspx\">Home</a></li>");
			indexPage.AppendLine("                <li Class=\"active\">");
			indexPage.AppendLine("                  <a href=\"#\">Conference</a>");
			indexPage.AppendLine("                  <ul>");
			indexPage.AppendLine("                    <li><a href=\"/Schedule/08-19-2020\">Schedule</a></li>");
			indexPage.AppendLine("                    <li><a href=\"/Speakers\">Speakers</a></li>");
			indexPage.AppendLine("                    <li><a href=\"/SessionsByTag/1\">Sessions by Tag</a></li>");
			indexPage.AppendLine("                    <li><a href=\"/SessionsByTopic/1\">Sessions by Topic</a></li>");
			indexPage.AppendLine("                    <li><a href=\"/Conference/SpeakerSeries.aspx\">Speaker Interview Series</a></li>");
			indexPage.AppendLine("                  </ul>");
			indexPage.AppendLine("                </li>");
			indexPage.AppendLine("                <li>");
			indexPage.AppendLine("                  <a href=\"/Sponsors/Default.aspx\">Sponsors</a>");
			indexPage.AppendLine("                </li>");
			indexPage.AppendLine("              </ul>");
			indexPage.AppendLine("            </nav>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("          <div class=\"span2\">");
			indexPage.AppendLine("            <ul class=\"list list-inline pull-right\">");
			indexPage.AppendLine("              <li>");
			indexPage.AppendLine("                <a href=\"https://www.facebook.com/CodePaLOUsa\" class=\"icon-facebook box-icon-gray round box-icon-to-normal animate-icon-top-to-bottom\"></a>");
			indexPage.AppendLine("              </li>");
			indexPage.AppendLine("              <li>");
			indexPage.AppendLine("                <a href=\"https://twitter.com/CodePaLOUsa\" class=\"icon-twitter box-icon-gray round box-icon-to-normal animate-icon-top-to-bottom\"></a>");
			indexPage.AppendLine("              </li>");
			indexPage.AppendLine("            </ul>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("        </div>");
			indexPage.AppendLine("      </div>");
			indexPage.AppendLine("    </header>");
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
			indexPage.AppendLine($"          <h2 id=\"MainContent_MainContent_SessionTitle\" class=\"SessionDetails\">{session.Title}</h3>");
			indexPage.AppendLine($"          <h5 id=\"MainContent_MainContent_SessionType\" class=\"SessionDetails\">{session.SessionType}</h4>");
			indexPage.AppendLine("          <br />");
			indexPage.AppendLine("          <div class=\"smart-player-embed-container\">");
			indexPage.AppendLine("            <iframe class=\"smart-player-embed-iframe\" id=\"embeddedSmartPlayerInstance\" src=\"player.html\" scrolling=\"no\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine($"          {session.Abstract}");
			indexPage.AppendLine("          <hr />");
			indexPage.AppendLine("          <div class=\"row row-wrap\">");
			indexPage.AppendLine("            <div class=\"span3\">");
			foreach (var speaker in session.SessionSpeakers)
			{
				indexPage.AppendLine("              <div class=\"thumb center\">");
				indexPage.AppendLine("                <div class=\"thumb-header\">");
				indexPage.AppendLine($"                  <a class=\"hover-img\" href=\"/SpeakerDetails/{speaker.Id}\">");
				indexPage.AppendLine($"                    <img src=\"https://greeneventstechnology.azureedge.net/cpl20/speakers/{speaker.FirstName}_{speaker.LastName}.png\" alt=\"{speaker.FirstName} {speaker.LastName}\" title=\"{speaker.FirstName} {speaker.LastName}\" />");
				indexPage.AppendLine("                  </a>");
				indexPage.AppendLine("                </div>");
				indexPage.AppendLine("                <div class=\"thumb-caption\">");
				indexPage.AppendLine($"                  <h5 class=\"thumb-title\"><a href=\"/SpeakerDetails/{speaker.Id}\">{speaker.FirstName} {speaker.LastName}</a></h5>");
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
			indexPage.AppendLine("    <footer class=\"main\">");
			indexPage.AppendLine("      <div class=\"container\">");
			indexPage.AppendLine("        <div class=\"row\">");
			indexPage.AppendLine("          <div class=\"span2\">");
			indexPage.AppendLine("            <a id=\"HyperLink1\" class=\"logo\" href=\"http://localhost:55989/Default.aspx\"><img src=\"https://greeneventstechnology.azureedge.net/cpl20/cpl20/img/logo/HeaderLogo.png\" alt=\"\" /></a>");
			indexPage.AppendLine("            <div class=\"gap gap-small\"></div>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("          <div class=\"span3 offset1\">");
			indexPage.AppendLine("            <h5>About</h5>");
			indexPage.AppendLine("            <p>A software development conference in the Louisville, KY area on August 19 - 21, 2020 designed to cover all aspects of software development regardless of development stack.</p>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("          <div class=\"span3\">");
			indexPage.AppendLine("            <h5>Keep in Touch</h5>");
			indexPage.AppendLine("            <ul class=\"list list-inline\">");
			indexPage.AppendLine("              <li>");
			indexPage.AppendLine("                <a class=\"icon-facebook box-icon-inverse box-icon-to-normal round animate-icon-left-to-right\" href=\"https://www.facebook.com/CodePaLOUsa\"></a>");
			indexPage.AppendLine("              </li>");
			indexPage.AppendLine("              <li>");
			indexPage.AppendLine("                <a class=\"icon-twitter box-icon-inverse box-icon-to-normal round animate-icon-left-to-right\" href=\"https://twitter.com/CodePaLOUsa\"></a>");
			indexPage.AppendLine("              </li>");
			indexPage.AppendLine("            </ul>");
			indexPage.AppendLine("            <br />");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine("        </div>");
			indexPage.AppendLine("      </div>");
			indexPage.AppendLine("    </footer>");
			indexPage.AppendLine("    <div class=\"copyright center\">");
			indexPage.AppendLine("      <p>Copyright &#169; 2020 <a href=\"#\">Code PaLOUsa</a>. All Right Reserved.</p>");
			indexPage.AppendLine("    </div>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/jquery.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/easing.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/touch-swipe.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/boostrap.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/flexnav.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/countdown.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/magnific.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/mediaelement.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/fitvids.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/gridrotator.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/fredsel.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/backgroundsize.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/superslides.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/one-page-nav.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/scroll-to.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/com/maps/api/js?sensor=false\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/gmap3.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/tweet.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/mixitup.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/mail.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/transit-modified.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/layerslider-transitions.min.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/layerslider.js\"></script>");
			indexPage.AppendLine("    <script src=\"https://greeneventstechnology.azureedge.net/cpl20/js/custom.js\"></script>");
			indexPage.AppendLine("  </div>");
			indexPage.AppendLine("</body>");
			indexPage.AppendLine("</html>");
			return indexPage.ToString();
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

		private static string BuildSchedulePageHeader(int sessionPeriodId)
		{
			var header = new StringBuilder();
			header.AppendLine("<div class=\"top-title-area bg-img-charcoal-eticket\">");
			header.AppendLine("  <div class=\"container\">");
			header.AppendLine("    <h1 class=\"title-page\">Schedule</h1>");
			header.AppendLine("  </div>");
			header.AppendLine("</div>");
			header.AppendLine("<div class=\"gap\"></div>");
			header.AppendLine("<div class=\"container\">");
			header.AppendLine("  <div class=\"demo-buttons\">");
			if (sessionPeriodId == 105)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Workshops</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Workshops</a>");
			if (sessionPeriodId == 108)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 1</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 1</a>");
			if (sessionPeriodId == 109)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 2</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 2</a>");
			if (sessionPeriodId == 110)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 3</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 3</a>");
			if (sessionPeriodId == 111)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 4</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 4</a>");
			if (sessionPeriodId == 112)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 5</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 5</a>");
			if (sessionPeriodId == 113)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 6</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 6</a>");
			if (sessionPeriodId == 114)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 7</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 7</a>");
			if (sessionPeriodId == 115)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 8</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 8</a>");
			if (sessionPeriodId == 116)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 9</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 9</a>");
			if (sessionPeriodId == 117)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 10</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 10</a>");
			if (sessionPeriodId == 118)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 11</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Session Period 11</a>");
			if (sessionPeriodId == 119)
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Keynote</a>");
			else
				header.AppendLine("    <a asp-page=\"ScheduleWS\" class=\"btn\">Keynote</a>");
			header.AppendLine("  </div>");
			header.AppendLine("  <div class=\"gap\"></div>");
			return header.ToString();
		}

		private static void BuildSchedulePages(List<Session> sessions, string outputPath)
		{

			var sessionPeriodPages = new Dictionary<int, StringBuilder>();
			sessionPeriodPages.Add(105, new StringBuilder(BuildSchedulePageHeader(105)));
			sessionPeriodPages.Add(108, new StringBuilder(BuildSchedulePageHeader(108)));
			sessionPeriodPages.Add(109, new StringBuilder(BuildSchedulePageHeader(109)));
			sessionPeriodPages.Add(110, new StringBuilder(BuildSchedulePageHeader(110)));
			sessionPeriodPages.Add(111, new StringBuilder(BuildSchedulePageHeader(111)));
			sessionPeriodPages.Add(112, new StringBuilder(BuildSchedulePageHeader(112)));
			sessionPeriodPages.Add(113, new StringBuilder(BuildSchedulePageHeader(113)));
			sessionPeriodPages.Add(114, new StringBuilder(BuildSchedulePageHeader(114)));
			sessionPeriodPages.Add(115, new StringBuilder(BuildSchedulePageHeader(115)));
			sessionPeriodPages.Add(116, new StringBuilder(BuildSchedulePageHeader(116)));
			sessionPeriodPages.Add(117, new StringBuilder(BuildSchedulePageHeader(117)));
			sessionPeriodPages.Add(118, new StringBuilder(BuildSchedulePageHeader(118)));
			sessionPeriodPages.Add(119, new StringBuilder(BuildSchedulePageHeader(119)));

			foreach (var session in sessions)
			{
				var sessionListing = new StringBuilder();
				sessionListing.AppendLine("  <div class=\"row\">");
				sessionListing.AppendLine("    <div class=\"span4\">");
				sessionListing.AppendLine($"      <a asp-page=\"{session.Id}\">");
				sessionListing.AppendLine($"        <img style=\"width: 320px; height: 180px\" src=\"https://greeneventstechnology.azureedge.net/cpl20/thumbnails/{session.SessionPeriodId}.jpg\" />");
				sessionListing.AppendLine("       </a>");
				sessionListing.AppendLine("    </div>");
				sessionListing.AppendLine("    <div class=\"span8\">");
				sessionListing.AppendLine($"      <a asp-page=\"{session.Id}\">");
				sessionListing.AppendLine($"      <h3>{session.Title}</h3>");
				sessionListing.AppendLine($"        {session.Summary}");
				sessionListing.AppendLine("       </a>");
				sessionListing.AppendLine("    </div>");
				sessionListing.AppendLine("  </div>");
				sessionListing.AppendLine("  <hr />");
				sessionPeriodPages[session.SessionPeriodId].Append(sessionListing.ToString());
			}

			foreach (var sessionPeriodPage in sessionPeriodPages)
			{
				sessionPeriodPage.Value.AppendLine("</div>");
				var path = $@"{outputPath}sessions\Schedule";
				if (sessionPeriodPage.Key == 105)
					File.WriteAllText($"{path}WS.cshtml", sessionPeriodPage.ToString());

				if (sessionPeriodPage.Key == 108)
				{
					File.WriteAllText($"{path}01.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}01.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 109)
				{
					File.WriteAllText($"{path}02.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}02.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 110)
				{
					File.WriteAllText($"{path}03.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}03.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 111)
				{
					File.WriteAllText($"{path}04.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}04.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 112)
				{
					File.WriteAllText($"{path}05.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}05.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 113)
				{
					File.WriteAllText($"{path}06.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}06.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 114)
				{
					File.WriteAllText($"{path}07.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}07.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 115)
				{
					File.WriteAllText($"{path}08.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}08.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 116)
				{
					File.WriteAllText($"{path}09.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}09.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 117)
				{
					File.WriteAllText($"{path}10.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}10.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 118)
				{
					File.WriteAllText($"{path}11.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}11.cs", GetCSFile("sessions", "Schedule01"));
				}
				if (sessionPeriodPage.Key == 119)
				{
					File.WriteAllText($"{path}KN.cshtml", sessionPeriodPage.ToString());
					File.WriteAllText($"{path}KN.cs", GetCSFile("sessions", "Schedule01"));
				}
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