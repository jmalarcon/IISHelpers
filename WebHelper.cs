﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;

namespace IISHelpers
{
    internal static class WebHelper
    {
        /// <summary>
        /// Returns a param from web.config or a default value for it
        /// The defaultValue can be skipped and it will be returned an empty string if it's needed
        /// </summary>
        /// <param name="paramName">The name of the param to search in the configuration file</param>
        /// <param name="defaultvalue">The default value to return if the param is not found. It's optional. If missing it will return an empty string</param>
        /// <returns></returns>
        internal static string GetParamValue(string paramName, string defaultvalue = "")
        {
            string v = WebConfigurationManager.AppSettings[paramName];
            return String.IsNullOrEmpty(v) ? defaultvalue : v.Trim();
        }

        /// <summary>
        /// Searchs for virtual paths ("~/") and transform them to absolute paths (relative to the root of the server)
        /// </summary>
        /// <param name="content">The content to transform</param>
        /// <returns></returns>
        internal static string TransformVirtualPaths(string content)
        {
            string absoluteBase = VirtualPathUtility.ToAbsolute("~/");
            content = content.Replace("~/", absoluteBase);
            //Markdig codifies the "~" as "%7E" , so we need to process it this way too
            return content.Replace("%7E/", absoluteBase);
        }

        //Gets current user IP (even if it's forwarded by a proxy
        internal static string GetIPAddress()
        {
            HttpContext ctx = HttpContext.Current;

            //List of headers that can contain forwarding info if the connection is through a proxy
            string[] ForwardedHeaders = new string[] {
                "HTTP_X_FORWARDED_FOR", "HTTP_FORWARDED", "HTTP_FORWARDED_FOR",
                "HTTP_CLIENT-IP", "HTTP_CLIENT_IP", "HTTP_FORWARDED", "HTTP_X_FORWARDED", "HTTP_X_COMING_FROM",
                "HTTP_X_REAL_IP", "HTTP_VIA", "HTTP_COMING_FROM", "HTTP_FROM", "HTTP_PROXY_CONNECTION",
                "HTTP_X_CLUSTER_CLIENT_IP", "HTTP_True-Client-IP" };

            //Check the headers to see if the connection comes through a proxy
            foreach (string header in ForwardedHeaders)
            { 
                var headervalue = ctx.Request.ServerVariables[header];

                if (!string.IsNullOrEmpty(headervalue))
                {
                    string[] addresses = headervalue.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[addresses.Length-1];   //If there's more than one, the last one has the originating IP
                    }
                }
            }

            return ctx.Request.ServerVariables["REMOTE_ADDR"]; //Default common value
        }

        /// <summary>
        /// Returns the absolute path (from the web app's root folder) from the physical path of a file
        /// The file should be one inside the app's folder
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(string physicalPath)
        {
            string baseAppPath = HttpContext.Current.Request.PhysicalApplicationPath;
            return physicalPath.Replace(baseAppPath, "/").Replace(@"\", "/");
        }

        /// <summary>
        /// Gets the containing directory for the current file specified by it's VirtualPath
        /// </summary>
        /// <param name="virtPath">The virtual path for the file</param>
        /// <returns>The virtual path for the current file's container directory, with the ending slash</returns>
        public static string GetContainingDir(string virtPath)
        {
            int slashPos = virtPath.LastIndexOf('/');
            //If there's no directory
            if (slashPos == -1)
                return "/";    //Return the base folder
                               //If it's a path ending with a slash, so already a directory
            if (slashPos == virtPath.Length - 1)
                return virtPath;    //Return the original path
                                    //Else, remove the part from the last slash
            return virtPath.Substring(0, slashPos + 1);
        }
    }
}
