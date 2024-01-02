using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Helpers
{
    public static class MenuHelper
    {
        public static XElement ExportTree()
        {
            return ModuleManager.ExportTree;
        }

        public static XElement GetElement(this XElement menuTree, string id)
        {
            return menuTree.Elements("Export").FirstOrDefault(i => i.Attribute("id").Value == id);
        }

        /// <summary>
        /// defaul menubar load with bar name like 
        /// menubarRoot
        /// Accountbar
        /// </summary>
        /// <param name="menuBar"></param>
        /// <returns></returns>
        public static List<MenuItem> MenuBar(string menuBar)
        {
            List<MenuItem> menuItems = new List<MenuItem>();

            // get Infos of menu tree from Module Manager
            var menuBarRoot = ModuleManager.ExportTree.GetElement(menuBar);

            // go throw each item
            foreach (var menuBarItem in menuBarRoot.Elements())
            {
                if (menuBarItem != null)
                {
                    MenuItem menuItem = new MenuItem();
                    //get title of the item
                    menuItem.Title = menuBarItem.Attribute("title").Value;

                    //when item has children it will be no link but have link as childrens
                    if (menuBarItem.HasElements)
                    {
                        foreach (var child in menuBarItem.Elements())
                        {
                            StringBuilder sb = new StringBuilder();

                            MenuItem childItem = new MenuItem();
                            childItem.Title = getTitle(child);
                            childItem.Url = getUrl(child);
                            childItem.Module = getModule(child);
                            menuItem.Items.Add(childItem);
                        }
                    }
                    else
                    {
                        menuItem.Url = getUrl(menuBarItem);
                        menuItem.Module = getModule(menuBarItem);
                    }

                    menuItems.Add(menuItem);
                }
            }

            return menuItems;

        }

        public static List<MenuItem> MenuBarSecured(string menuBar,string userName, bool sortByModule=false)
        {
            List<MenuItem> menuItems = new List<MenuItem>();

            // get Infos of menu tree from Module Manager
            var menuBarRoot = ModuleManager.ExportTree.GetElement(menuBar);
            //// order by area and order number from module xml

            // go throw each item
            foreach (var menuBarItem in menuBarRoot.Elements())
            {
                if (menuBarItem != null)
                {
                    MenuItem menuItem = new MenuItem();
                    //get title of the item
                    menuItem.Title = menuBarItem.Attribute("title").Value;

                    //when item has children it will be no link but have link as childrens
                    if (menuBarItem.HasElements)
                    {
                        bool childaccess = false;
                        foreach (var child in menuBarItem.Elements())
                        {
                            if (hasOperationRigths(child, userName))
                            {
                                childaccess = true;

                                MenuItem childItem = new MenuItem();
                                childItem.Title = getTitle(child);
                                childItem.Url = getUrl(child);
                                childItem.Module = getModule(child);
                                menuItem.Items.Add(childItem);
                            }
                        }

                        if (childaccess) menuItems.Add(menuItem);
                    }
                    else
                    {
                        if (hasOperationRigths(menuBarItem, userName))
                        {
                            menuItem.Url = getUrl(menuBarItem);
                            menuItem.Module = getModule(menuBarItem);
                            menuItems.Add(menuItem);
                        }
                    }

                }
            }

            if(sortByModule) return menuItems.OrderBy(i=>i.Module).ToList();
            
            return menuItems;
        }

        public static List<MenuItem> AccountBar(bool isAuthenticated, string userName)
        {
            List<MenuItem> menuItems = new List<MenuItem>();


            // if the user is authenticated create the account menu 
            if (isAuthenticated)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Title = userName;

                //create profile link
                MenuItem profile = new MenuItem("Profile", "/bam/partyservice/edit", "bam");
                menuItem.Items.Add(profile);

                //create token link
                MenuItem token = new MenuItem("Token", "/account/profile", "shell");
                menuItem.Items.Add(token);

                //create api link
                MenuItem api = new MenuItem("Api", "/apihelp/index", "shell", "_blank");
                menuItem.Items.Add(api);

                //create logoff link
                MenuItem logoff = new MenuItem("Log Off", "/account/logoff", "shell");
                menuItem.Items.Add(logoff);

                menuItems.Add(menuItem);
            }
            else
            {
                MenuItem register = new MenuItem("Register","account/register","shell");
                MenuItem login = new MenuItem("Login","account/login", "shell");

                menuItems.Add(register);
                menuItems.Add(login);
            }

            

            return menuItems;
        }

        public static List<MenuItem> ExtendedMenu(XElement list)
        {
            // get current Tennat and load all extended menu parts
            List<MenuItem> menuItems = new List<MenuItem>();

            // get Infos of menu tree from Module Manager
            var menuBarRoot = list;

            // order by area and order number from module xml
            var children = menuBarRoot.Elements();

            // go throw each item
            foreach (var menuBarItem in children.Elements())
            {
                if (menuBarItem != null)
                {
                    MenuItem menuItem = new MenuItem();
                    //get title of the item
                    menuItem.Title = menuBarItem.Attribute("title").Value;

                    //when item has children it will be no link but have link as childrens
                    if (menuBarItem.HasElements)
                    {
                        foreach (var child in menuBarItem.Elements())
                        {
                            StringBuilder sb = new StringBuilder();

                            MenuItem childItem = new MenuItem();
                            childItem.Title = getTitle(child);
                            childItem.Module = getModule(child);
                            childItem.Url = getUrl(child);
                            childItem.Target = "_blank";
                            menuBarItem.Add(childItem);
                        }
                    }
                    else
                    {
                        menuItem.Url = getUrl(menuBarItem);
                    }

                    menuItems.Add(menuItem);
                }
            }

            return menuItems;
        }



        private static string getUrl(XElement element)
        {
            StringBuilder sb = new StringBuilder();


            if (!string.IsNullOrWhiteSpace(element.Attribute("area").Value))
                sb.Append(@"/").Append(element.Attribute("area").Value.ToLower());

            if (!string.IsNullOrWhiteSpace(element.Attribute("controller").Value))
                sb.Append(@"/").Append(element.Attribute("controller").Value.ToLower());

            if (!string.IsNullOrWhiteSpace(element.Attribute("action").Value))
                sb.Append(@"/").Append(element.Attribute("action").Value.ToLower());

            return sb.ToString();
        }

        private static string getModule(XElement element)
        {
            StringBuilder sb = new StringBuilder();


            if (!string.IsNullOrWhiteSpace(element.Attribute("area").Value))
                return element.Attribute("area").Value;

            return "";
        }

        private static string getTitle(XElement element)
        {
            if (element == null) return "";
            return element.Attribute("title")!=null? element.Attribute("title").Value:"";
        }

        private static bool hasOperationRigths(XElement operation, string userName)
        {
            //get parameters for the function to check
            string name = userName;
            string area = operation.Attribute("area").Value.ToLower();
            string controller = operation.Attribute("controller").Value.ToLower();

            string identifier = name + "_" + area + "_" + controller;

            // check if rights already stored in the session
            if (System.Web.HttpContext.Current.Session["menu_permission"] != null && ((Dictionary<string, bool>)System.Web.HttpContext.Current.Session["menu_permission"]).ContainsKey(identifier))
            {
                return (bool)((Dictionary<string, bool>)System.Web.HttpContext.Current.Session["menu_permission"])[identifier];
            }

            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
            OperationManager operationManager = new OperationManager();

            try
            {

                //currently the action are not check, so we use a wildcard
                string action = "*";//operation.Attribute("action").Value.ToLower();

                if (string.IsNullOrEmpty(operation.Attribute("area").Value)) area = "shell";
                else area = operation.Attribute("area").Value.ToLower();

                bool permission = featurePermissionManager.HasAccess<User>(name, area, controller, action);

                System.Web.HttpContext.Current.Session[identifier] = permission;

                // check if dictionary for menu permissions exists and create it if not
                if (System.Web.HttpContext.Current.Session["menu_permission"] == null)
                {
                    System.Web.HttpContext.Current.Session["menu_permission"] = new Dictionary<string, bool>();
                }

                ((Dictionary<string, bool>)System.Web.HttpContext.Current.Session["menu_permission"]).Add(identifier, permission); // add menu right for the currently logged in user to session

                return permission;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                featurePermissionManager.Dispose();
                operationManager.Dispose();
            }
        }



    }

}