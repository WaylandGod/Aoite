﻿using Aoite.CommandModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace System.Web
{
    /// <summary>
    /// 表示一个 Web Application 的增强功能。
    /// </summary>
    public class Webx
    {
        #region Temp Values

        /// <summary>
        /// 指定一个名称，获取当前请求的临时数据。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="name">名称。</param>
        /// <returns>如果存在返回值，否则返回默认值。</returns>
        public static T GetTemp<T>(string name)
        {
            return Webx.GetTemp<T>(name, null);
        }
        /// <summary>
        /// 指定一个名称和默认值，获取当前请求的临时数据。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="name">名称。</param>
        /// <param name="defaultValue">自定义的默认值。</param>
        /// <returns>如果存在返回值，否则返回默认值。</returns>
        public static T GetTemp<T>(string name, T defaultValue)
        {
            return Webx.GetTemp<T>(name, null);
        }
        /// <summary>
        /// 指定一个名称和默认值回调方法，获取当前请求的临时数据。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="name">名称。</param>
        /// <param name="defaultValueCallback">自定义的默认值的回调方法。</param>
        /// <returns>如果存在返回值，否则执行回调方法并返回默认值。</returns>
        public static T GetTemp<T>(string name, Func<T> defaultValueCallback)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            var items = HttpContext.Current.Items;
            var value = items[name];
            if(value == null)
            {
                if(defaultValueCallback == null) return default(T);
                var tValue = defaultValueCallback();
                items[name] = tValue;
                return tValue;
            }

            return (T)value;
        }
        /// <summary>
        /// 设置当前请求的临时数据。
        /// </summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="name">名称。</param>
        /// <param name="value">值。</param>
        /// <returns>返回设置的值。</returns>
        public static T SetTemp<T>(string name, T value)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            var items = HttpContext.Current.Items;
            if(value == null) items.Remove(name);
            else items[name] = value;
            return value;
        }

        #endregion

        #region Url & Path

        /// <summary>
        /// 返回一个包含内容 URL 的字符串。
        /// </summary>
        /// <param name="contentPath">内容路径。</param>
        /// <returns>一个包含内容 URL 的字符串。</returns>
        public static string MapUrl(string contentPath)
        {
            if(string.IsNullOrEmpty(contentPath)) throw new ArgumentNullException("contentPath");
            var path = HttpRuntime.AppDomainAppVirtualPath ?? "/";
            return VirtualPathUtility.Combine(path, VirtualPathUtility.ToAbsolute(contentPath, path));
        }

        /// <summary>
        /// 将虚拟路径映射到服务器上的物理路径。
        /// </summary>
        /// <param name="virtualPath">虚拟路径（绝对路径或相对路径）。</param>
        /// <returns>由 <paramref name="virtualPath"/> 指定的服务器物理路径。</returns>
        public static string MapPath(string virtualPath)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);
        }

        #endregion

        #region Cookie

        private readonly static string RootPath = Webx.MapUrl("~/");

        /// <summary>
        /// 新建或更新客户端的 Cookie。
        /// </summary>
        /// <param name="name">新 Cookie 的名称。</param>
        /// <param name="value">新 Cookie 的值。如果值为 null 值，则表示移除该项。</param>
        /// <param name="path">要与当前 Cookie 一起传输的虚拟路径。</param>
        /// <param name="httpOnly">指定 Cookie 是否可通过客户端脚本访问。</param>
        public static void Cookie(string name, string value, string path = null, bool httpOnly = false)
        {
            Cookie(name, value, DateTime.Now.AddDays(3), path ?? RootPath, httpOnly);
        }

        /// <summary>
        /// 新建或更新客户端的 Cookie。
        /// </summary>
        /// <param name="name">新 Cookie 的名称。</param>
        /// <param name="value">新 Cookie 的值。如果值为 null 值，则表示移除该项。</param>
        /// <param name="expires">此 Cookie 的过期日期和时间。</param>
        /// <param name="path">要与当前 Cookie 一起传输的虚拟路径。</param>
        /// <param name="httpOnly">指定 Cookie 是否可通过客户端脚本访问。</param>
        public static void Cookie(string name, string value, DateTime expires, string path = null, bool httpOnly = false)
        {
            var ctx = HttpContext.Current;
            var request = ctx.Request;
            var response = ctx.Response;

            var cookie = response.Cookies[name];
            cookie.Value = value;
            cookie.Expires = value == null ? DateTime.Now.AddYears(-1) : expires;
            cookie.HttpOnly = httpOnly;
            if(path != null) cookie.Path = path;
        }

        /// <summary>
        /// 指定名称，获取客户端的 Cookie 值。
        /// </summary>
        /// <param name="name">Cookie 的名称。</param>
        /// <returns>返回 Cookie 的值。默认值为 <see cref="System.String.Empty"/>。</returns>
        public static string Cookie(string name)
        {
            var cookie = HttpContext.Current.Request.Cookies[name];
            if(cookie == null || cookie.Value == null) return string.Empty;
            return cookie.Value;
        }

        /// <summary>
        /// 指定名称，移除客户端的 Cookie 值。
        /// </summary>
        /// <param name="name">Cookie 的名称。</param>
        /// <returns>返回 Cookie 的值。默认值为 <see cref="System.String.Empty"/>。</returns>
        public static string CookieRemove(string name)
        {
            var value = Cookie(name);
            Cookie(name, null, DateTime.Now);
            return value;
        }

        /// <summary>
        /// 移除客户端的所有 Cookie。
        /// </summary>
        public static void CookieClear()
        {
            var ctx = HttpContext.Current;
            var response = ctx.Response;
            foreach(var key in ctx.Request.Cookies.AllKeys)
            {
                response.Cookies[key].Expires = DateTime.Now.AddYears(-1);
            }
        }

        #endregion

        #region Scripts

        private const string SCRIPT_STRING_BUILDER = "$Webx:SCRIPT_STRING_BUILDER$";
        private readonly static IHtmlString EmptyHtmlString = new HtmlString(string.Empty);
        /// <summary>
        /// 添加客户端脚本。
        /// </summary>
        /// <param name="scripts">脚本的内容。</param>
        public static void AppendScripts(string scripts)
        {
            if(string.IsNullOrEmpty(scripts)) return;
            var scriptBuilder = GetTemp<Text.StringBuilder>(SCRIPT_STRING_BUILDER, () => new Text.StringBuilder());

            using(var reader = new System.IO.StringReader(scripts))
            {
                string line;
                while((line = reader.ReadToEnd()) != null)
                {
                    scriptBuilder.Append("            ");
                    scriptBuilder.AppendLine(line);
                }
            }
        }

        /// <summary>
        /// 呈现所有已添加的脚本，并清空脚本。
        /// </summary>
        /// <returns>返回一个脚本字符串。</returns>
        public static IHtmlString ReaderScripts()
        {
            var scriptBuilder = GetTemp<Text.StringBuilder>(SCRIPT_STRING_BUILDER);
            if(scriptBuilder == null) return EmptyHtmlString;
            var content = scriptBuilder.ToString();
            return new HtmlString(
@"    <script type=""text/javascript"">
        $(function () {" + content + @"        });    </script>");
        }

        #endregion

        #region Commom

        private const string IsJsonResponseName = "$Webx:IS_JSON_RESPONSE";
        /// <summary>
        /// 获取一个值，指示客户端是否要求 JSON 的响应。
        /// </summary>
        public static bool IsJsonResponse
        {
            get
            {
                var isJsonResult = GetTemp<bool?>(IsJsonResponseName, defaultValue: null);
                if(isJsonResult == null)
                {
                    var request = HttpContext.Current.Request;
                    isJsonResult = ((request["X-Requested-With"] == "XMLHttpRequest")
                        || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest")));

                    SetTemp(IsJsonResponseName, isJsonResult);
                }
                return isJsonResult.Value;
            }
        }

        #endregion

        #region Identity

        private const string ContainerName = "$Webx:Container";
        private static IIocContainer SetContainer(IIocContainer value)
        {
            var config = value.GetValue("$AppSettings") as NameValueCollection
                  ?? System.Web.Configuration.WebConfigurationManager.AppSettings
                  ?? new NameValueCollection();

            var redisAddress = config.Get("reids.address");
            if(!string.IsNullOrEmpty(redisAddress))
            {
                var sp = redisAddress.Split(':');
                if(sp.Length != 2) throw new ArgumentException("非法的 Redis 的连接地址 {0}。".Fmt(redisAddress));
                Aoite.Redis.RedisManager.DefaultAddress = new Aoite.Net.SocketInfo(sp[0], int.Parse(sp[1]));
            }
            Aoite.Redis.RedisManager.DefaultPassword = config.Get("redis.password");

            if(config.Get<bool>("redis.enabled")) value.AddService<IRedisProvider>(new RedisProvider(value));

            if(!value.ContainsService<IUserFactory>(true)) value.AddService<IUserFactory>(value.GetService<IIdentityStore>());

            HttpContext.Current.Application[ContainerName] = value;
            return value;
        }

        private static IIocContainer GetContainer()
        {
            return HttpContext.Current.Application[ContainerName] as IIocContainer;
        }

        /// <summary>
        /// 获取或设置用于 Webx 的服务容器。
        /// </summary>
        public static IIocContainer Container
        {
            get
            {
                var container = GetContainer();
                if(container == null)
                    lock(string.Intern(ContainerName))
                    {
                        return GetContainer() ?? SetContainer(ObjectFactory.Global);
                    }
                return container;
            }
            set
            {
                if(value == null) throw new ArgumentNullException("value");
                SetContainer(value);
            }
        }

        /// <summary>
        /// 获取一个值，指示当前请求是否已通过授权。
        /// </summary>
        public static bool IsAuthorized { get { return Identity != null; } }

        private const string AllowAnonymousName = "$Webx:ALLOW_ANONYMOUS";
        /// <summary>
        /// 获取或设置一个值，指示当前请求是否允许匿名访问。
        /// </summary>
        public static bool AllowAnonymous { get { return GetTemp(AllowAnonymousName, false); } set { SetTemp(AllowAnonymousName, value); } }

        private const string IdentityName = "$Webx:IDENTITY";
        /// <summary>
        ///  获取或设置客户端唯一标识，如果上下文缓存不存在，则尝试从当前请求中获取。
        /// </summary>
        /// <returns>返回客户端唯一标识。</returns>
        public static dynamic Identity
        {
            get { return GetTemp<object>(IdentityName, Container.GetService<IIdentityStore>().Get); }
            set
            {
                var store = Container.GetService<IIdentityStore>();
                object v = value;
                if(v == null)
                {
                    store.Remove();
                    SetTemp<object>(IdentityName, null);
                }
                else
                {
                    store.Set(v);
                    SetTemp(IdentityName, v);
                }
            }
        }

        [SingletonMapping]
        internal class SessionIdentityStore : IIdentityStore
        {
            public void Set(object user)
            {
                HttpContext.Current.Session[IdentityName] = user;
            }

            public object Get()
            {
                return HttpContext.Current.Session[IdentityName];
            }

            public void Remove()
            {
                HttpContext.Current.Session.Remove(IdentityName);
            }

            public object GetUser(IIocContainer container)
            {
                return this.Get();
            }
        }

        #endregion
    }

}
