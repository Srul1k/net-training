using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace IOStreams
{

	public static class TestTasks
	{
		/// <summary>
		/// Parses Resourses\Planets.xlsx file and returns the planet data: 
		///   Jupiter     69911.00
		///   Saturn      58232.00
		///   Uranus      25362.00
		///    ...
		/// See Resourses\Planets.xlsx for details
		/// </summary>
		/// <param name="xlsxFileName">source file name</param>
		/// <returns>sequence of PlanetInfo</returns>
		public static IEnumerable<PlanetInfo> ReadPlanetInfoFromXlsx(string xlsxFileName)
		{
            using (Package package = Package.Open(xlsxFileName, FileMode.Open, FileAccess.Read))
            {
                PackagePart sharedStrings = package.GetPart(new Uri("/xl/sharedStrings.xml", UriKind.Relative));
                XDocument allStrings = XDocument.Load(sharedStrings.GetStream(), LoadOptions.PreserveWhitespace);
                var names = allStrings.Root
                    .Descendants()
                    .Where(e => e.Name.LocalName == "t")
					.Take(8)
                    .Select(x => x.Value);
				  
                PackagePart worksheets = package.GetPart(new Uri("/xl/worksheets/sheet1.xml", UriKind.Relative));
                XDocument colsAndRows = XDocument.Load(worksheets.GetStream(), LoadOptions.PreserveWhitespace);
                var radii = colsAndRows.Root
                    .Descendants()
                    .Where(e => e.Name.LocalName == "v" && e.Parent.Attribute("r").Value[0] == 'B')
					.Skip(1)
                    .Select(x => Double.Parse(x.Value, new System.Globalization.CultureInfo("us")))
                    .ToArray();

				return names.Zip(radii, (n, r) => new PlanetInfo() { Name = n, MeanRadius = r });
            }
        }


		/// <summary>
		/// Calculates hash of stream using specifued algorithm
		/// </summary>
		/// <param name="stream">source stream</param>
		/// <param name="hashAlgorithmName">hash algorithm ("MD5","SHA1","SHA256" and other supported by .NET)</param>
		/// <returns></returns>
		public static string CalculateHash(this Stream stream, string hashAlgorithmName)
		{
			HashAlgorithm hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);

			if (hashAlgorithm == null)
            {
				throw new ArgumentException($"{hashAlgorithmName} is not a valid hash algorithm.");
            }

			byte[] data = hashAlgorithm.ComputeHash(stream);
			return String.Concat(data.Select(b => b.ToString("X2")));
		}


		/// <summary>
		/// Returns decompressed strem from file. 
		/// </summary>
		/// <param name="fileName">source file</param>
		/// <param name="method">method used for compression (none, deflate, gzip)</param>
		/// <returns>output stream</returns>
		public static Stream DecompressStream(string fileName, DecompressionMethods method)
		{
			var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			var decompress = CompressionMode.Decompress;

			switch(method)
            {
				case DecompressionMethods.GZip:
					return new GZipStream(fileStream, decompress);
				case DecompressionMethods.Deflate:
					return new DeflateStream(fileStream, decompress);
				default:
					return fileStream;
            }
		}


		/// <summary>
		/// Reads file content econded with non Unicode encoding
		/// </summary>
		/// <param name="fileName">source file name</param>
		/// <param name="encoding">encoding name</param>
		/// <returns>Unicoded file content</returns>
		public static string ReadEncodedText(string fileName, string encoding)
		{
			return File.ReadAllText(fileName, Encoding.GetEncoding(encoding));
		}
	}


	public class PlanetInfo : IEquatable<PlanetInfo>
	{
		public string Name { get; set; }
		public double MeanRadius { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1}", Name, MeanRadius);
		}

		public bool Equals(PlanetInfo other)
		{
			return Name.Equals(other.Name)
				&& MeanRadius.Equals(other.MeanRadius);
		}
	}



}
