using System.Collections.Generic;

namespace BExIS.UI.Models
{
    public class Menu
    {
        public Logo Logo { get; set; }

        public List<MenuItem> LaunchBar { get; set; }
        public List<MenuItem> MenuBar { get; set; }
        public List<MenuItem> AccountBar { get; set; }
        public List<MenuItem> Settings { get; set; }
        public List<MenuItem> Extended { get; set; }

        public Menu()
        {
            Logo = new Logo();
            LaunchBar = new List<MenuItem>();
            MenuBar = new List<MenuItem>();
            AccountBar = new List<MenuItem>();
            Settings = new List<MenuItem>();
            Extended = new List<MenuItem>();
        }
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public string Module { get; set; }
        public List<MenuItem> Items { get; set; }

        public MenuItem()
        {
            Title = "";
            Url = "";
            Items = new List<MenuItem>();
            Target = "_self";
            Module = "";
        }

        public MenuItem(string title, string url, string module, string target = "_self")
        {
            Title = title;
            Url = url;
            Items = new List<MenuItem>();
            Target = target;
            Module = module;
        }
    }

    public class Logo
    {
        public string Mime { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public int Height { get; set; }

        public Logo()
        {
            Mime = "";
            Name = "";
            Data = "";
            Height = 0;
        }

        public Logo(string name, string mime, string data, int height)
        {
            Mime = mime;
            Name = name;
            Data = data;
            Height = height;
        }
    }
}