using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SIFDemo
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());

			//String _body = "<environment><solutionId>testSolution</solutionId><authenticationMethod>Basic</authenticationMethod><applicationInfo><applicationKey>MIKER_TEST</applicationKey><consumerName>MIKER_CONSUMERNAME</consumerName><supportedInfrastructureVersion>3.0</supportedInfrastructureVersion><supportedDataModel>SIF-US</supportedDataModel><supportedDataModelVersion>3.0</supportedDataModelVersion><transport>REST</transport><applicationProduct><vendorName>X</vendorName><productName>X</productName><productVersion>X</productVersion></applicationProduct></applicationInfo></environment>";

			//createEnvironmentAndGetToken();

			//Console.ReadLine();
		}

		private const string BASEURL = "http://rest3api.sifassociation.org/api";


		static string _createTestEnvBody = @"<environment>
								  <solutionId>testSolution</solutionId>
								  <authenticationMethod>Basic</authenticationMethod>
								  <instanceId></instanceId>
								  <userToken>new</userToken>
								  <consumerName>guest</consumerName>
								  <applicationInfo>
									<applicationKey>MyApp</applicationKey>
									<supportedInfrastructureVersion>3.0</supportedInfrastructureVersion>
									<supportedDataModel>SIF-US</supportedDataModel>
									<supportedDataModelVersion>3.0</supportedDataModelVersion>
									<transport>REST</transport>
									<applicationProduct>
									  <vendorName>VDS</vendorName>
									  <productName>MyVDSProduct</productName>
									  <productVersion>1</productVersion>
									</applicationProduct>
								  </applicationInfo>
								</environment>";

		private static String createEnvironmentAndGetToken()
		{
			HttpWebRequest req = HttpWebRequest.CreateHttp(BASEURL + "/environments/environment");
			req.Credentials = new NetworkCredential("new", "guest");
			req.ContentType = "application/xml";
			req.Accept = "application/xml";
			req.Headers.Add("Authorization", "Basic bmV3Omd1ZXN0Cg==");
			req.Method = "POST";
			using (Stream s = req.GetRequestStream())
			{
				byte[] b = Encoding.UTF8.GetBytes(_createTestEnvBody);
				s.Write(b, 0, b.Length);
			}

			using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
			{
				Console.WriteLine(String.Format("Respone: {0}", res.StatusCode));
				using (StreamReader s = new StreamReader(res.GetResponseStream()))
				{
					Console.WriteLine(s.ReadToEnd());
				}
			}

			return string.Empty;
		}
	}
}
