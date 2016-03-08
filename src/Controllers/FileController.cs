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

using Zongsoft.IO;
using Zongsoft.Common;

namespace Zongsoft.Web.Controllers
{
	public class FileController : ApiController
	{
		#region 常量定义
		private const string EXTENDED_PROPERTY_PREFIX = "x-zfs-";
		#endregion

		#region 成员字段
		private string _basePath;
		#endregion

		#region 构造函数
		public FileController()
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
		/// <summary>
		/// 下载指定路径的文件。
		/// </summary>
		/// <param name="path">指定要下载的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public HttpResponseMessage Get(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var stream = FileSystem.File.Open(this.GetFilePath(path), FileMode.Open, FileAccess.Read, properties);

			if(stream == null)
				throw new HttpResponseException(HttpStatusCode.NotFound);

			//创建当前文件的流内容
			var content = new StreamContent(stream);

			//设置返回的内容头信息
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
			content.Headers.ContentType = new MediaTypeHeaderValue(System.Web.MimeMapping.GetMimeMapping(path));

			if(properties.Count > 0)
			{
				string fileName, lastModified;

				if(properties.TryGetValue("FileName", out fileName))
					content.Headers.ContentDisposition.FileName = fileName;

				if(properties.TryGetValue("HTTP:LastModified", out lastModified))
				{
					DateTimeOffset modifiedTime;

					if(Zongsoft.Common.Convert.TryConvertValue<DateTimeOffset>(lastModified, out modifiedTime))
						content.Headers.LastModified = modifiedTime;
				}
			}

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content,
			};
		}

		/// <summary>
		/// 获取指定文件的外部访问路径。
		/// </summary>
		/// <param name="path">指定的文件相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回指定文件的外部访问路径。</returns>
		[HttpGet]
		public string Path(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return FileSystem.GetUrl(this.GetFilePath(path));
		}

		/// <summary>
		/// 获取指定路径的文件描述信息。
		/// </summary>
		/// <param name="path">指定要获取的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回的指定的文件详细信息。</returns>
		[HttpGet]
		public async Task<Zongsoft.IO.FileInfo> Info(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return await FileSystem.File.GetInfoAsync(this.GetFilePath(path));
		}

		/// <summary>
		/// 修改指定路径的文件描述信息。
		/// </summary>
		/// <param name="path">指定要修改的文件相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public async Task<bool> Put(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach(var header in this.Request.Headers)
			{
				if(header.Key.Length > EXTENDED_PROPERTY_PREFIX.Length && header.Key.StartsWith(EXTENDED_PROPERTY_PREFIX, StringComparison.OrdinalIgnoreCase))
					properties[header.Key.Substring(EXTENDED_PROPERTY_PREFIX.Length)] = string.Join("", header.Value);
			}

			if(this.Request.Content.IsFormData())
			{
				var form = await this.Request.Content.ReadAsFormDataAsync();

				foreach(string fieldName in form)
				{
					if(fieldName.Length > EXTENDED_PROPERTY_PREFIX.Length && fieldName.StartsWith(EXTENDED_PROPERTY_PREFIX, StringComparison.OrdinalIgnoreCase))
						properties[fieldName.Substring(EXTENDED_PROPERTY_PREFIX.Length)] = form[fieldName];
				}
			}

			if(properties.Count > 0)
				return await FileSystem.File.SetInfoAsync(this.GetFilePath(path), properties);

			return false;
		}

		/// <summary>
		/// 删除指定相对路径的文件。
		/// </summary>
		/// <param name="path">指定要删除的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public async Task<bool> Delete(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return await FileSystem.File.DeleteAsync(this.GetFilePath(path));
		}

		/// <summary>
		/// 新增一个文件或多个文件。
		/// </summary>
		/// <param name="path">指定新增文件的目录路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回新增文件的<see cref="Zongsoft.IO.FileInfo"/>描述信息实体对象集。</returns>
		public async Task<IEnumerable<Zongsoft.IO.FileInfo>> Post(string path = null)
		{
			var directoryPath = this.GetFilePath(path);

			//检测请求的内容是否为Multipart类型
			if(!this.Request.Content.IsMimeMultipartContent("form-data"))
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

			//创建自定义头的字典
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			//构建自定义头的字典内容
			foreach(var header in this.Request.Headers)
			{
				if(header.Key.Length > EXTENDED_PROPERTY_PREFIX.Length && header.Key.StartsWith(EXTENDED_PROPERTY_PREFIX, StringComparison.OrdinalIgnoreCase))
					headers[header.Key.Substring(EXTENDED_PROPERTY_PREFIX.Length)] = string.Join("", header.Value);
			}

			//创建多段表单信息的文件流操作的供应程序
			var provider = new MultipartStorageFileStreamProvider(directoryPath, headers);

			//从当前请求内容读取多段信息并写入文件中
			var result = await this.Request.Content.ReadAsMultipartAsync(provider);

			if(result.FormData != null && result.FormData.Count > 0)
			{
				foreach(var fileEntry in result.FileData)
				{
					var prefix = EXTENDED_PROPERTY_PREFIX + fileEntry.Key + "-";
					var updateRequires = false;

					foreach(var formEntry in result.FormData)
					{
						if(formEntry.Key.Length > prefix.Length && formEntry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
						{
							updateRequires = true;
							fileEntry.Value.Properties[formEntry.Key.Substring(prefix.Length)] = formEntry.Value;
						}
					}

					if(updateRequires)
						await FileSystem.File.SetInfoAsync(fileEntry.Value.Path.Url, fileEntry.Value.Properties);
				}
			}

			//返回新增的文件信息实体集
			return result.FileData.Values;
		}
		#endregion

		#region 私有方法
		private string GetFilePath(string path)
		{
			var basePath = this.BasePath;

			if(string.IsNullOrWhiteSpace(basePath))
				throw new InvalidOperationException("Missing the base-path of file system.");

			var schema = Zongsoft.IO.Path.GetScheme(basePath);

			if(string.IsNullOrWhiteSpace(schema))
				throw new InvalidOperationException(string.Format("Invalid format of the '{0}' base-path.", basePath));

			if(string.IsNullOrWhiteSpace(path))
				return basePath;

			path = Uri.UnescapeDataString(path).Trim();

			if(path.StartsWith("/"))
				return schema + ":" + path;
			else
				return Zongsoft.IO.Path.Combine(basePath, path);
		}
		#endregion

		#region 嵌套子类
		internal class MultipartStorageFileStreamProvider : MultipartStreamProvider
		{
			#region 成员字段
			private string _directoryPath;
			private IDictionary<string, string> _headers;
			private IDictionary<string, Zongsoft.IO.FileInfo> _fileData;
			private IDictionary<string, string> _formData;
			private Collection<bool> _isFormData;
			#endregion

			#region 构造函数
			public MultipartStorageFileStreamProvider(string directoryPath, IDictionary<string, string> headers)
			{
				if(string.IsNullOrWhiteSpace(directoryPath))
					throw new ArgumentNullException("directoryPath");

				_directoryPath = directoryPath;
				_headers = headers;
				_fileData = new Dictionary<string, Zongsoft.IO.FileInfo>();
				_isFormData = new Collection<bool>();
				_formData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
			#endregion

			#region 公共属性
			public string DirectoryPath
			{
				get
				{
					return _directoryPath;
				}
			}

			public IDictionary<string, Zongsoft.IO.FileInfo> FileData
			{
				get
				{
					return _fileData;
				}
			}

			public IDictionary<string, string> FormData
			{
				get
				{
					return _formData;
				}
			}
			#endregion

			#region 重写方法
			public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
			{
				if(parent == null)
					throw new ArgumentNullException("parent");

				if(headers == null)
					throw new ArgumentNullException("headers");

				//判断当前内容项是否为普通表单域，如果是则返回一个内存流
				if(headers.ContentDisposition == null || string.IsNullOrEmpty(headers.ContentDisposition.FileName))
				{
					//在表单数据标记列表中按顺序将当前内容标记为普通表单域
					_isFormData.Add(true);

					//返回一个新的内存流
					return new MemoryStream();
				}

				//获取当前内容项的内容类型
				var contentType = headers.ContentType == null ? string.Empty : headers.ContentType.MediaType;

				if(string.IsNullOrWhiteSpace(contentType))
					contentType = System.Web.MimeMapping.GetMimeMapping(headers.ContentDisposition.FileName);

				//在表单数据标记列表中按顺序将当前内容标记为非普通表单域（即二进制文件域）
				_isFormData.Add(false);

				string fileName = null;
				var dispositionName = this.UnquoteToken(headers.ContentDisposition.Name);

				if(!string.IsNullOrWhiteSpace(dispositionName))
				{
					//获取请求头中显式指定的文件名（注意：该文件名支持模板格式）
					if(_headers.TryGetValue(dispositionName + ".name", out fileName))
						fileName = Zongsoft.Text.TemplateEvaluatorManager.Default.Evaluate<string>(fileName, null).ToLowerInvariant() + System.IO.Path.GetExtension(headers.ContentDisposition.FileName.Trim('"'));
				}

				//如果文件名为空，则生成一个以“当前日期-时间-随机数.ext”的默认文件名
				if(string.IsNullOrWhiteSpace(fileName))
					fileName = string.Format("{0:yyyyMMdd-HHmmss}-{1}{2}", DateTime.Now, (uint)Zongsoft.Common.RandomGenerator.GenerateInt32(), System.IO.Path.GetExtension(headers.ContentDisposition.FileName.Trim('"')));

				//生成文件的完整路径
				var filePath = Zongsoft.IO.Path.Combine(_directoryPath, fileName);

				//生成文件信息的描述实体
				Zongsoft.IO.FileInfo fileInfo = new Zongsoft.IO.FileInfo(filePath, (headers.ContentDisposition.Size.HasValue ? headers.ContentDisposition.Size.Value : -1), DateTime.Now, null, FileSystem.GetUrl(filePath));

				//将上传的原始文件名加入到文件描述实体的扩展属性中
				fileInfo.Properties.Add("FileName", Uri.UnescapeDataString(headers.ContentDisposition.FileName.Trim('"')));

				if(_headers != null && _headers.Count > 0 && !string.IsNullOrWhiteSpace(dispositionName))
				{
					//从全局头里面查找当前上传文件的自定义属性
					foreach(var header in _headers)
					{
						if(header.Key.Length > dispositionName.Length + 1 && header.Key.StartsWith(dispositionName + "-", StringComparison.OrdinalIgnoreCase))
							fileInfo.Properties.Add(header.Key.Substring(dispositionName.Length + 1), header.Value);
					}
				}

				var infoKey = string.IsNullOrWhiteSpace(dispositionName) ? fileName : dispositionName;

				//将文件信息对象加入到集合中
				_fileData.Add(infoKey, fileInfo);

				try
				{
					//调用文件系统根据完整文件路径去创建一个新建文件流
					return FileSystem.File.Open(filePath, FileMode.CreateNew, FileAccess.Write, (fileInfo.HasProperties ? fileInfo.Properties : null));
				}
				catch
				{
					if(fileInfo != null)
						_fileData.Remove(infoKey);

					throw;
				}
			}

			public override async Task ExecutePostProcessingAsync()
			{
				int index = 0;

				foreach(var content in this.Contents)
				{
					if(_isFormData[index++])
					{
						_formData.Add(this.UnquoteToken(content.Headers.ContentDisposition.Name), await content.ReadAsStringAsync());
					}
					else
					{
						if(content.Headers.ContentDisposition != null && content.Headers.ContentDisposition.Size.HasValue)
						{
							Zongsoft.IO.FileInfo info;

							if(_fileData.TryGetValue(this.UnquoteToken(content.Headers.ContentDisposition.Name), out info))
								info.Size = content.Headers.ContentDisposition.Size.Value;
						}
					}
				}
			}
			#endregion

			#region 私有方法
			private string UnquoteToken(string token)
			{
				if(string.IsNullOrWhiteSpace(token))
					return string.Empty;

				if(token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
					return token.Substring(1, token.Length - 2);

				return token.Trim();
			}
			#endregion
		}
		#endregion
	}
}
