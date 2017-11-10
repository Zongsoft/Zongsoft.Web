/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Zongsoft.Web.Http.Controllers
{
	public class FileController : ApiController
	{
		#region 成员字段
		private WebFileAccessor _accessor;
		#endregion

		#region 构造函数
		public FileController()
		{
		}
		#endregion

		#region 公共属性
		[Zongsoft.Services.ServiceDependency]
		public WebFileAccessor Accessor
		{
			get
			{
				return _accessor;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_accessor = value;
			}
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 下载指定路径的文件。
		/// </summary>
		/// <param name="path">指定要下载的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public HttpResponseMessage Get(string path)
		{
			if(string.IsNullOrEmpty(path))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			return _accessor.Read(path);
		}

		/// <summary>
		/// 获取指定文件的外部访问路径。
		/// </summary>
		/// <param name="path">指定的文件相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回指定文件的外部访问路径。</returns>
		[HttpGet]
		public string Path(string path)
		{
			if(string.IsNullOrEmpty(path))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			return _accessor.GetUrl(path);
		}

		/// <summary>
		/// 获取指定路径的文件描述信息。
		/// </summary>
		/// <param name="path">指定要获取的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回的指定的文件详细信息。</returns>
		[HttpGet]
		public Task<Zongsoft.IO.FileInfo> Info(string path)
		{
			if(string.IsNullOrEmpty(path))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			return _accessor.GetInfo(path);
		}

		/// <summary>
		/// 删除指定相对路径的文件。
		/// </summary>
		/// <param name="path">指定要删除的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public Task<bool> Delete(string path)
		{
			if(string.IsNullOrEmpty(path))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			return _accessor.Delete(path);
		}

		/// <summary>
		/// 修改指定路径的文件描述信息。
		/// </summary>
		/// <param name="path">指定要修改的文件相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public Task<bool> Put(string path)
		{
			if(string.IsNullOrEmpty(path))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			return _accessor.SetInfo(this.Request, path);
		}

		/// <summary>
		/// 上传一个文件或多个文件。
		/// </summary>
		/// <param name="directory">指定上传文件的目录路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回上传成功的<see cref="Zongsoft.IO.FileInfo"/>文件描述信息实体对象集。</returns>
		public Task<IEnumerable<Zongsoft.IO.FileInfo>> Post(string directory = null)
		{
			return _accessor.Write(this.Request, directory);
		}
		#endregion
	}
}
