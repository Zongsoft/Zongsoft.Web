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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

using Zongsoft.IO;

namespace Zongsoft.Web
{
	[Zongsoft.Services.Matcher(typeof(Zongsoft.IO.FileSystem.Matcher))]
	public class WebFileSystem : Zongsoft.IO.IFileSystem
	{
		#region 单例字段
		public static readonly WebFileSystem Instance = new WebFileSystem();
		#endregion

		#region 构造函数
		private WebFileSystem()
		{
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取Web文件目录系统的方案，始终返回“zfs.web”。
		/// </summary>
		public string Scheme
		{
			get
			{
				return "zfs.web";
			}
		}

		public IFile File
		{
			get
			{
				return WebFileProvider.Instance;
			}
		}

		public IDirectory Directory
		{
			get
			{
				return WebDirectoryProvider.Instance;
			}
		}
		#endregion

		#region 公共方法
		public string GetUrl(string path)
		{
			if(string.IsNullOrEmpty(path))
				return null;

			var url = HttpContext.Current.Request.Url;
			return url.Scheme + "://" + url.Authority + path;
		}

		public string GetUrl(Zongsoft.IO.Path path)
		{
			if(path == null)
				return null;

			var url = HttpContext.Current.Request.Url;
			return url.Scheme + "://" + url.Authority + path.FullPath;
		}

		public static string GetPhysicalPath(string virtualPath)
		{
			if(string.IsNullOrWhiteSpace(virtualPath))
				return HttpContext.Current.Server.MapPath("~");

			return HttpContext.Current.Server.MapPath(virtualPath);
		}
		#endregion

		#region 嵌套子类
		private class WebDirectoryProvider : IDirectory
		{
			#region 单例字段
			public static readonly WebDirectoryProvider Instance = new WebDirectoryProvider();
			#endregion

			#region 公共方法
			public bool Create(string virtualPath, IDictionary<string, object> properties = null)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.Create(physicalPath, properties);
			}

			public Task<bool> CreateAsync(string virtualPath, IDictionary<string, object> properties = null)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.CreateAsync(physicalPath, properties);
			}

			public bool Delete(string virtualPath, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.Delete(physicalPath, recursive);
			}

			public Task<bool> DeleteAsync(string virtualPath, bool recursive)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.DeleteAsync(physicalPath, recursive);
			}

			public void Move(string source, string destination)
			{
				var physicalSource = WebFileSystem.GetPhysicalPath(source);
				var physicalDestination = WebFileSystem.GetPhysicalPath(destination);
				LocalFileSystem.Instance.Directory.Move(physicalSource, physicalDestination);
			}

			public Task MoveAsync(string source, string destination)
			{
				var physicalSource = WebFileSystem.GetPhysicalPath(source);
				var physicalDestination = WebFileSystem.GetPhysicalPath(destination);
				return LocalFileSystem.Instance.Directory.MoveAsync(physicalSource, physicalDestination);
			}

			public bool Exists(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.Exists(physicalPath);
			}

			public Task<bool> ExistsAsync(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.ExistsAsync(physicalPath);
			}

			public DirectoryInfo GetInfo(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetInfo(physicalPath);
			}

			public Task<DirectoryInfo> GetInfoAsync(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetInfoAsync(physicalPath);
			}

			public bool SetInfo(string virtualPath, IDictionary<string, object> properties)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.SetInfo(physicalPath, properties);
			}

			public Task<bool> SetInfoAsync(string virtualPath, IDictionary<string, object> properties)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.SetInfoAsync(physicalPath, properties);
			}

			public IEnumerable<PathInfo> GetChildren(string virtualPath)
			{
				return this.GetChildren(virtualPath, null, false);
			}

			public IEnumerable<PathInfo> GetChildren(string virtualPath, string pattern, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetChildren(physicalPath, pattern, recursive);
			}

			public Task<IEnumerable<PathInfo>> GetChildrenAsync(string virtualPath)
			{
				return this.GetChildrenAsync(virtualPath, null, false);
			}

			public Task<IEnumerable<PathInfo>> GetChildrenAsync(string virtualPath, string pattern, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetChildrenAsync(physicalPath, pattern, recursive);
			}

			public IEnumerable<DirectoryInfo> GetDirectories(string virtualPath)
			{
				return this.GetDirectories(virtualPath, null, false);
			}

			public IEnumerable<DirectoryInfo> GetDirectories(string virtualPath, string pattern, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetDirectories(physicalPath, pattern, recursive);
			}

			public Task<IEnumerable<DirectoryInfo>> GetDirectoriesAsync(string virtualPath)
			{
				return this.GetDirectoriesAsync(virtualPath, null, false);
			}

			public Task<IEnumerable<DirectoryInfo>> GetDirectoriesAsync(string virtualPath, string pattern, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetDirectoriesAsync(physicalPath, pattern, recursive);
			}

			public IEnumerable<FileInfo> GetFiles(string virtualPath)
			{
				return this.GetFiles(virtualPath, null, false);
			}

			public IEnumerable<FileInfo> GetFiles(string virtualPath, string pattern, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetFiles(physicalPath, pattern, recursive);
			}

			public Task<IEnumerable<FileInfo>> GetFilesAsync(string virtualPath)
			{
				return this.GetFilesAsync(virtualPath, null, false);
			}

			public Task<IEnumerable<FileInfo>> GetFilesAsync(string virtualPath, string pattern, bool recursive = false)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.Directory.GetFilesAsync(physicalPath, pattern, recursive);
			}
			#endregion
		}

		private class WebFileProvider : IFile
		{
			#region 单例字段
			public static readonly WebFileProvider Instance = new WebFileProvider();
			#endregion

			#region 公共方法
			public bool Delete(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.Delete(physicalPath);
			}

			public Task<bool> DeleteAsync(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.DeleteAsync(physicalPath);
			}

			public bool Exists(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.Exists(physicalPath);
			}

			public Task<bool> ExistsAsync(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.ExistsAsync(physicalPath);
			}

			public void Copy(string source, string destination)
			{
				this.Copy(source, destination, true);
			}

			public void Copy(string source, string destination, bool overwrite)
			{
				var physicalSource = WebFileSystem.GetPhysicalPath(source);
				var physicalDestination = WebFileSystem.GetPhysicalPath(destination);
				LocalFileSystem.Instance.File.Copy(physicalSource, physicalDestination, overwrite);
			}

			public Task CopyAsync(string source, string destination)
			{
				return this.CopyAsync(source, destination, true);
			}

			public Task CopyAsync(string source, string destination, bool overwrite)
			{
				var physicalSource = WebFileSystem.GetPhysicalPath(source);
				var physicalDestination = WebFileSystem.GetPhysicalPath(destination);
				return LocalFileSystem.Instance.File.CopyAsync(physicalSource, physicalDestination, overwrite);
			}

			public void Move(string source, string destination)
			{
				var physicalSource = WebFileSystem.GetPhysicalPath(source);
				var physicalDestination = WebFileSystem.GetPhysicalPath(destination);
				LocalFileSystem.Instance.File.Move(physicalSource, physicalDestination);
			}

			public Task MoveAsync(string source, string destination)
			{
				var physicalSource = WebFileSystem.GetPhysicalPath(source);
				var physicalDestination = WebFileSystem.GetPhysicalPath(destination);
				return LocalFileSystem.Instance.File.MoveAsync(physicalSource, physicalDestination);
			}

			public FileInfo GetInfo(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.GetInfo(physicalPath);
			}

			public Task<FileInfo> GetInfoAsync(string virtualPath)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.GetInfoAsync(physicalPath);
			}

			public bool SetInfo(string virtualPath, IDictionary<string, object> properties)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.SetInfo(physicalPath, properties);
			}

			public Task<bool> SetInfoAsync(string virtualPath, IDictionary<string, object> properties)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.SetInfoAsync(physicalPath, properties);
			}

			public System.IO.Stream Open(string virtualPath, IDictionary<string, object> properties = null)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);
				return LocalFileSystem.Instance.File.Open(physicalPath, properties);
			}

			public System.IO.Stream Open(string virtualPath, System.IO.FileMode mode, IDictionary<string, object> properties = null)
			{
				var physicalPath = this.EnsureDirecotry(virtualPath, mode);
				return LocalFileSystem.Instance.File.Open(physicalPath, mode, properties);
			}

			public System.IO.Stream Open(string virtualPath, System.IO.FileMode mode, System.IO.FileAccess access, IDictionary<string, object> properties = null)
			{
				var physicalPath = this.EnsureDirecotry(virtualPath, mode);
				return LocalFileSystem.Instance.File.Open(physicalPath, mode, access, properties);
			}

			public System.IO.Stream Open(string virtualPath, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share, IDictionary<string, object> properties = null)
			{
				var physicalPath = this.EnsureDirecotry(virtualPath, mode);
				return LocalFileSystem.Instance.File.Open(physicalPath, mode, access, share, properties);
			}
			#endregion

			#region 私有方法
			private string EnsureDirecotry(string virtualPath, System.IO.FileMode mode)
			{
				var physicalPath = WebFileSystem.GetPhysicalPath(virtualPath);

				if(mode != System.IO.FileMode.Open)
				{
					var directory = System.IO.Path.GetDirectoryName(physicalPath);

					if(!System.IO.Directory.Exists(directory))
						System.IO.Directory.CreateDirectory(directory);
				}

				return physicalPath;
			}
			#endregion
		}
		#endregion
	}
}
