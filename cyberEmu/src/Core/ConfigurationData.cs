using System;
using System.Collections.Generic;
using System.IO;
namespace Cyber.Core
{
	internal class ConfigurationData
	{
		internal Dictionary<string, string> data;
		internal bool fileHasBeenRead;

		internal ConfigurationData(string filePath, bool maynotexist = false)
		{
			this.data = new Dictionary<string, string>();
			if (File.Exists(filePath))
			{
				this.fileHasBeenRead = true;
				try
				{
					using (StreamReader streamReader = new StreamReader(filePath))
					{
						string text;
						while ((text = streamReader.ReadLine()) != null)
						{
							if (text.Length >= 1 && !text.StartsWith("#"))
							{
								int num = text.IndexOf('=');
								if (num != -1)
								{
									string key = text.Substring(0, num);
									string value = text.Substring(checked(num + 1));
									this.data.Add(key, value);
								}
							}
						}
						streamReader.Close();
					}
				}
				catch (Exception ex)
				{
					throw new ArgumentException("Could not process configuration file: " + ex.Message);
				}
				return;
			}
			if (!maynotexist)
			{
				throw new ArgumentException("No se encontró el archivo de config. en '" + filePath + "'.");
			}
		}
	}
}
