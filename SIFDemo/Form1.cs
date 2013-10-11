using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SIFDemo
{
	public partial class Form1 : Form
	{		
		private string _envID;
		private string _environmentURL;
		private string _queuesURL;

		private string _sesssionToken;
		private String SessionToken
		{
			get { return _sesssionToken; }
			set
			{
				textBoxSessionToken.Text = value;
				_sesssionToken = value;
			}
		}

		public Form1()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Debug.Listeners.Add(new MyListener(textBox1));
		}

		private void button1_Click(object sender, EventArgs e)
		{
			CreateEnvironmentAndGetSessionToken();
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

		private void Clean()
		{
			SessionToken = null;
			_environmentURL = null;
			_envID = null;
			_queuesURL = null;
		}

		private bool CreateEnvironmentAndGetSessionToken()
		{
			Clean();

			HttpWebRequest req = HttpWebRequest.CreateHttp(BASEURL + "/environments/environment");
			req.ContentType = "application/xml";
			req.Accept = "application/xml";
			req.Headers.Add("Authorization", "Basic bmV3Omd1ZXN0Cg==");
			req.Method = "POST";
			using (Stream s = req.GetRequestStream())
			{
				byte[] b = Encoding.UTF8.GetBytes(_createTestEnvBody);
				s.Write(b, 0, b.Length);
			}

			HttpStatusCode ret = HttpStatusCode.BadRequest;
			using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
			{
				Debug.WriteLine(String.Format("Respone: {0}", res.StatusCode));
				String responseBody;
				using (StreamReader s = new StreamReader(res.GetResponseStream()))
					responseBody = s.ReadToEnd();
				Debug.WriteLine(GetPrettyXML(responseBody));

				ret = res.StatusCode;
				if (res.StatusCode == HttpStatusCode.Created)
				{
					//grab info
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(responseBody);
					_envID = doc.SelectSingleNode("/environment").Attributes["id"].Value;
					SessionToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(doc.SelectSingleNode("/environment/sessionToken").InnerText + ":guest"));//guest needs to change when not demo

					foreach (XmlNode n in doc.SelectNodes("/environment/infrastructureServices/infrastructureService "))
					{
						switch (n.Attributes["name"].Value)
						{
							case "environment":
								_environmentURL = n.InnerText;
								break;
							case "queues":
								_queuesURL = n.InnerText;
								break;
							default:
								break;
						}//end switch
					}
				}//end if
			}//end using

			return ret == HttpStatusCode.Created;
		}

		private string GetPrettyXML(string xml)
		{			
			XmlDocument document = new XmlDocument();
			String result = null;

			using(MemoryStream mStream = new MemoryStream())
			using(XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode))
			{
				try
				{
					document.LoadXml(xml);
					writer.Formatting = Formatting.Indented;
					document.WriteContentTo(writer);
					writer.Flush();
					mStream.Flush();

					mStream.Position = 0;
					StreamReader sReader = new StreamReader(mStream);
					result = sReader.ReadToEnd();
				}
				catch (XmlException)
				{
				}
			}//end using

			return result;
		}

		class MyListener : TraceListener
		{
			private TextBox _tb;
			public MyListener(TextBox tb)
			{
				_tb = tb;
			}

			public override void Write(object o)
			{
				base.Write(o);
				Write(o.ToString());
			}

			public override void Write(string message)
			{
				_tb.Text += message;
			}

			public override void WriteLine(object o)
			{
				base.WriteLine(o);
				WriteLine(o.ToString());
			}

			public override void WriteLine(string message)
			{
				_tb.Text += message + Environment.NewLine;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			TearDown();
		}

		private void TearDown()
		{
			if (String.IsNullOrEmpty(_environmentURL))
				return;

			HttpWebRequest req = HttpWebRequest.CreateHttp(_environmentURL);
			req.ContentType = "application/xml";
			req.Accept = "application/xml";
			req.Headers.Add("Authorization", "Basic " + SessionToken);
			req.Method = "DELETE";

			using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
			{
				if (res.StatusCode == HttpStatusCode.NoContent)
				{
					Clean();
					Debug.WriteLine("Result - No Content (success)");
				}//end if

				using (StreamReader s = new StreamReader(res.GetResponseStream()))
					Debug.WriteLine(GetPrettyXML(s.ReadToEnd()));
			}//end using
		}

		private void button3_Click(object sender, EventArgs e)
		{
			textBox1.Text = String.Empty;
		}

		private List<Student> GetStudents()
		{
			if (SessionToken == null)
				return null;

			List<Student> students = new List<Student>();

			HttpWebRequest req = HttpWebRequest.CreateHttp(BASEURL + "/students");
			req.ContentType = "application/xml";
			req.Accept = "application/xml";
			req.Headers.Add("Authorization", "Basic " + SessionToken);
			req.Method = "GET";

			using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
			{
				if (res.StatusCode == HttpStatusCode.OK)
				{
					XmlDocument doc = new XmlDocument();
					using (StreamReader r = new StreamReader(res.GetResponseStream()))
						doc.LoadXml(r.ReadToEnd());

					foreach (XmlNode n in doc.FirstChild.NextSibling.SelectNodes("student"))
					{
						students.Add(new Student() { Name = n.SelectSingleNode("name/nameOfRecord/fullName").InnerText });
					}

				}//end if
			}//end using

			return students;
		}

		private struct Student
		{
			public string Name { get; set; }
		}

		private void button4_Click(object sender, EventArgs e)
		{
			listBox1.Items.Clear();

			if (SessionToken == null)
				return;

			foreach (Student s in GetStudents())
			{
				listBox1.Items.Add(s.Name);
			}
		}

	}
}
