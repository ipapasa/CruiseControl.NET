using System.Web.UI;
using System.Web.UI.HtmlControls;
using NMock;
using NMock.Constraints;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.MVC.View;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.AddProject;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.BuildReport;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.DeleteProject;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.EditProject;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.ViewServerLog;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class DefaultUserRequestSpecificSideBarViewBuilderTest
	{
		private DynamicMock urlBuilderMock;
		private DynamicMock buildNameRetrieverMock;
		private DynamicMock recentBuildsViewBuilderMock;
		private DynamicMock buildPluginLinkCalculatorMock;
		private DefaultUserRequestSpecificSideBarViewBuilder viewBuilder;

		[SetUp]
		public void Setup()
		{
			urlBuilderMock = new DynamicMock(typeof(IUrlBuilder));
			buildNameRetrieverMock = new DynamicMock(typeof(IBuildNameRetriever));
			recentBuildsViewBuilderMock = new DynamicMock(typeof(IRecentBuildsViewBuilder));
			buildPluginLinkCalculatorMock = new DynamicMock(typeof(IBuildPluginLinkCalculator));
			viewBuilder = new DefaultUserRequestSpecificSideBarViewBuilder(new DefaultHtmlBuilder(), 
				(IUrlBuilder) urlBuilderMock.MockInstance, 
				(IBuildNameRetriever) buildNameRetrieverMock.MockInstance,
				(IRecentBuildsViewBuilder) recentBuildsViewBuilderMock.MockInstance,
				(IBuildPluginLinkCalculator) buildPluginLinkCalculatorMock.MockInstance);
		}

		private void VerifyAll()
		{
			urlBuilderMock.Verify();
			buildNameRetrieverMock.Verify();
			recentBuildsViewBuilderMock.Verify();
			buildPluginLinkCalculatorMock.Verify();
		}

		[Test]
		public void ShouldReturnLinkToAddProjectForFarmView()
		{
			// Setup
			urlBuilderMock.ExpectAndReturn("BuildUrl", "returnedurl", new PropertyIs("ActionName", DisplayAddProjectPageAction.ACTION_NAME));

			// Execute
			HtmlTable table = viewBuilder.GetFarmSideBar();
			HtmlAnchor anchor = new HtmlAnchor();
			anchor.HRef = "returnedurl";
			anchor.InnerHtml = "Add Project";

			Assert.IsTrue(TableContains(table, anchor));
			
			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldReturnLinkToAddProjectAndServerLogForServerView()
		{
			// Setup
			urlBuilderMock.ExpectAndReturn("BuildServerUrl", "returnedurl1", new PropertyIs("ActionName", ViewServerLogAction.ACTION_NAME), "myServer");
			urlBuilderMock.ExpectAndReturn("BuildServerUrl", "returnedurl2", new PropertyIs("ActionName", DisplayAddProjectPageAction.ACTION_NAME), "myServer");
			HtmlAnchor expectedAnchor1 = new HtmlAnchor();
			expectedAnchor1.HRef = "returnedurl1";
			expectedAnchor1.InnerHtml = "View Server Log";
			HtmlAnchor expectedAnchor2 = new HtmlAnchor();
			expectedAnchor2.HRef = "returnedurl2";
			expectedAnchor2.InnerHtml = "Add Project";

			// Execute
			HtmlTable table = viewBuilder.GetServerSideBar("myServer");

			Assert.IsTrue(TableContains(table, expectedAnchor1));
			Assert.IsTrue(TableContains(table, expectedAnchor2));
			
			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldReturnLinkToProjectReportForProjectView()
		{
			// Setup
			urlBuilderMock.ExpectAndReturn("BuildProjectUrl", "editUrl", new PropertyIs("ActionName", DisplayEditProjectPageAction.ACTION_NAME), "myServer", "myProject");
			urlBuilderMock.ExpectAndReturn("BuildProjectUrl", "deleteUrl", new PropertyIs("ActionName", ShowDeleteProjectAction.ACTION_NAME), "myServer", "myProject");
			HtmlTable buildsPanel = new HtmlTable();
			recentBuildsViewBuilderMock.ExpectAndReturn("BuildRecentBuildsTable", buildsPanel, "myServer", "myProject");

			HtmlAnchor expectedAnchor1 = new HtmlAnchor();
			expectedAnchor1.HRef = "editUrl";
			expectedAnchor1.InnerHtml = "Edit Project";
			HtmlAnchor expectedAnchor2 = new HtmlAnchor();
			expectedAnchor2.HRef = "deleteUrl";
			expectedAnchor2.InnerHtml = "Delete Project";

			// Execute
			HtmlTable table = viewBuilder.GetProjectSideBar("myServer", "myProject");

			Assert.IsTrue(TableContains(table, expectedAnchor1));
			Assert.IsTrue(TableContains(table, expectedAnchor2));
			Assert.IsTrue(TableContains(table, buildsPanel));
			
			// Verify
			VerifyAll();
		}

		[Test]
		public void ShouldReturnCorrectLinksForBuildView()
		{
			// Setup
			buildNameRetrieverMock.ExpectAndReturn("GetLatestBuildName", "returnedLatestBuildName", "myServer", "myProject");
			buildNameRetrieverMock.ExpectAndReturn("GetNextBuildName", "returnedNextBuildName", "myServer", "myProject", "myCurrentBuild");
			buildNameRetrieverMock.ExpectAndReturn("GetPreviousBuildName", "returnedPreviousBuildName", "myServer", "myProject", "myCurrentBuild");

			Mock link1Mock = new DynamicMock(typeof(IAbsoluteLink));
			Mock link2Mock = new DynamicMock(typeof(IAbsoluteLink));
			link1Mock.SetupResult("Description", "my link 1");
			link1Mock.SetupResult("AbsoluteURL", "myurl1");
			link2Mock.SetupResult("Description", "my link 2");
			link2Mock.SetupResult("AbsoluteURL", "myurl2");

			buildPluginLinkCalculatorMock.ExpectAndReturn("GetBuildPluginLinks", new IAbsoluteLink[] { (IAbsoluteLink) link1Mock.MockInstance, (IAbsoluteLink) link2Mock.MockInstance }, "myServer", "myProject", "myCurrentBuild");
			urlBuilderMock.ExpectAndReturn("BuildBuildUrl", "latestUrl", new PropertyIs("ActionName", ViewBuildReportAction.ACTION_NAME), "myServer", "myProject", "returnedLatestBuildName");
			urlBuilderMock.ExpectAndReturn("BuildBuildUrl", "nextUrl", new PropertyIs("ActionName", ViewBuildReportAction.ACTION_NAME), "myServer", "myProject", "returnedNextBuildName");
			urlBuilderMock.ExpectAndReturn("BuildBuildUrl", "previousUrl", new PropertyIs("ActionName", ViewBuildReportAction.ACTION_NAME), "myServer", "myProject", "returnedPreviousBuildName");
			
			HtmlTable buildsPanel = new HtmlTable();
			recentBuildsViewBuilderMock.ExpectAndReturn("BuildRecentBuildsTable", buildsPanel, "myServer", "myProject");

			HtmlAnchor expectedAnchor1 = new HtmlAnchor();
			expectedAnchor1.HRef = "latestUrl";
			expectedAnchor1.InnerHtml = "Latest";
			HtmlAnchor expectedAnchor2 = new HtmlAnchor();
			expectedAnchor2.HRef = "nextUrl";
			expectedAnchor2.InnerHtml = "Next";
			HtmlAnchor expectedAnchor3 = new HtmlAnchor();
			expectedAnchor3.HRef = "previousUrl";
			expectedAnchor3.InnerHtml = "Previous";
			HtmlAnchor expectedAnchor4 = new HtmlAnchor();
			expectedAnchor4.HRef = "myurl1";
			expectedAnchor4.InnerHtml = "my link 1";
			HtmlAnchor expectedAnchor5 = new HtmlAnchor();
			expectedAnchor5.HRef = "myurl2";
			expectedAnchor5.InnerHtml = "my link 2";

			// Execute
			HtmlTable table = viewBuilder.GetBuildSideBar("myServer", "myProject", "myCurrentBuild");

			Assert.IsTrue(TableContains(table, expectedAnchor1));
			Assert.IsTrue(TableContains(table, expectedAnchor2));
			Assert.IsTrue(TableContains(table, expectedAnchor3));
			Assert.IsTrue(TableContains(table, expectedAnchor4));
			Assert.IsTrue(TableContains(table, expectedAnchor5));
			Assert.IsTrue(TableContains(table, buildsPanel));
			
			// Verify
			VerifyAll();
		}

		private bool TableContains(HtmlTable table, Control expectedControl)
		{
			foreach (HtmlTableRow row in table.Rows)
			{
				foreach (HtmlTableCell cell in row.Cells)
				{
					foreach (Control control in cell.Controls)
					{
						if (control is HtmlAnchor && expectedControl is HtmlAnchor)
						{
							HtmlAnchor currentAnchor = (HtmlAnchor) control;
							HtmlAnchor expectedAnchor = (HtmlAnchor) expectedControl;
							if (currentAnchor.HRef == expectedAnchor.HRef && currentAnchor.InnerHtml == expectedAnchor.InnerHtml)
							{
								return true;
							}
						}
						else if (control == expectedControl)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
