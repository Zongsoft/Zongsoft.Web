/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.
 *
 * Zongsoft.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

namespace Zongsoft.Web.Controllers
{
	public class DirectoryController : ApiController
	{
		#region 成员字段
		private string _basePath;
		#endregion

		#region 构造函数
		public DirectoryController()
		{
		}
		#endregion

		#region 公共属性
		public string BasePath
		{
			get
			{
				return _basePath;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				var text = value.Trim();

				_basePath = text + (text.EndsWith("/") ? string.Empty : "/");
			}
		}
		#endregion

		#region 公共方法
		public async Task<IEnumerable<Zongsoft.IO.PathInfo>> Get(string path, string pattern = null)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return await Zongsoft.IO.FileSystem.Directory.GetChildrenAsync(this.GetDirectoryPath(path), GetPattern(pattern));
		}

		[HttpGet]
		public async Task<IEnumerable<Zongsoft.IO.FileInfo>> Files(string path, string pattern = null)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return await Zongsoft.IO.FileSystem.Directory.GetFilesAsync(this.GetDirectoryPath(path), GetPattern(pattern));
		}
		#endregion

		#region 私有方法
		private string GetDirectoryPath(string path)
		{
			var basePath = this.BasePath;

			if(string.IsNullOrWhiteSpace(basePath))
				throw new InvalidOperationException("Missing the base-path of file system.");

			var schema = Zongsoft.IO.Path.GetSchema(basePath);

			if(string.IsNullOrWhiteSpace(schema))
				throw new InvalidOperationException(string.Format("Invalid format of the '{0}' base-path.", basePath));

			if(string.IsNullOrWhiteSpace(path))
				return basePath;

			path = Uri.UnescapeDataString(path).Trim();

			if(path.StartsWith("/"))
				return schema + ":" + path + (path.EndsWith("/") ? string.Empty : "/");
			else
				return Zongsoft.IO.Path.Combine(basePath, path) + (path.EndsWith("/") ? string.Empty : "/");
		}

		private string GetPattern(string pattern)
		{
			if(string.IsNullOrWhiteSpace(pattern))
				return null;

			return pattern.Trim();
		}
		#endregion
	}
}
