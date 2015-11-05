using System;
using System.Collections.Generic;

namespace Zongsoft.Web
{
	internal static class Utility
	{
		public static string RepairQueryString(string path, string queryString = null)
		{
			if(string.IsNullOrWhiteSpace(queryString))
				return path;

			queryString = queryString.Trim().TrimStart('?');

			if(string.IsNullOrWhiteSpace(path))
				return "/" + (string.IsNullOrWhiteSpace(queryString) ? string.Empty : "?" + queryString);

			var index = path.IndexOf('?');

			if(index > 0)
				return path + "&" + queryString;
			else
				return path + "?" + queryString;
		}
	}
}
