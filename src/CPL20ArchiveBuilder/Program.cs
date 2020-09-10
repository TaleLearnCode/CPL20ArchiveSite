using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace CPL20ArchiveBuilder
{
	class Program
	{

		static void Main()
		{


			using SqlConnection sqlConnection = new SqlConnection(Settings.DatabaseConnectionString);
			sqlConnection.Open();

			var speakers = Speakers.GetSpeakersForEvent(10, sqlConnection);
			var sessions = Session.GetSessionForEvent(10, sqlConnection, speakers);
			var sessionTags = SessionTags.GetTags(sqlConnection);

			Console.Clear();
			Console.WriteLine("Building session pages...");

			var rootDirectory = @"C:\Code PaLOUsa 2020 Videos\";
			var outputPath = @"D:\Repros\TaleLearnCode\CPL20ArchiveSite\src\CPL20Archive\wwwroot\";
			foreach (string sessionPeriodPath in Directory.GetDirectories(rootDirectory))
			{
				//Console.WriteLine(sessionPeriodPath);
				foreach (string sessionPath in Directory.GetDirectories(sessionPeriodPath))
				{
					var sessionPathComponents = sessionPath.Split('\\');
					if (sessions.ContainsKey(Convert.ToInt32(sessionPathComponents[sessionPathComponents.Length - 1])))
					{
						var session = sessions[Convert.ToInt32(sessionPathComponents[sessionPathComponents.Length - 1])];
						Console.WriteLine(session.Id);
						var path = $@"{outputPath}sessions\{session.Id}\";
						Directory.CreateDirectory(path);
						File.WriteAllText($"{path}index.html", BuildIndexPage(session, sessionTags));

					}
				}
			}



			sqlConnection.Close();

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
			indexPage.AppendLine("          <div class=\"smart-player-embed-container\">");
			indexPage.AppendLine("            <iframe class=\"smart-player-embed-iframe\" id=\"embeddedSmartPlayerInstance\" src=\"player.html\" scrolling=\"no\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>");
			indexPage.AppendLine("          </div>");
			indexPage.AppendLine($"          <h3 id=\"MainContent_MainContent_SessionTitle\" class=\"SessionDetails\">{session.Title}</h3>");
			indexPage.AppendLine($"          <h4 id=\"MainContent_MainContent_SessionType\" class=\"SessionDetails\">{session.SessionType}</h4>");
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

	}

}