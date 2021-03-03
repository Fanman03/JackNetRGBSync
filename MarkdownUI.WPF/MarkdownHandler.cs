using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarkdownUI.WPF
{
    public class MarkdownUIHandler
    {
        public MarkdownUIHandler()
        {

        }

        public MarkdownUIHandler(string body, MarkDownViewModel viewmodel, ResourceDictionary resourceDictionary)
        {
            Markdown = body;
            ViewModel = viewmodel;
            this.resourceDictionary = resourceDictionary;
        }

        private ResourceDictionary resourceDictionary;
        public string Markdown { get; set; }
        public MarkDownViewModel ViewModel { get; internal set; }
        public void Read(string path)
        {
            Markdown = File.ReadAllText(path);
        }

        public void Load(string body)
        {
            Markdown = body;
        }

        private void ProcessInline(TextBlock panel, MarkdownInline inline)
        {
            try
            {
                var tp = inline.GetType();
                // Debug.WriteLine(tp);

                switch (inline)
                {
                    case TextRunInline textRun:
                        {
                            panel.Inlines.Add(textRun.Text);
                            break;
                        }

                    case BoldTextInline boldText:
                        {
                            TextBlock tb = new TextBlock();
                            //tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownBold"));
                            tb.FontWeight = FontWeights.Bold;

                            foreach (var il in boldText.Inlines)
                            {
                                ProcessInline(tb, il);
                            }

                            panel.Inlines.Add(tb);

                            break;
                        }

                    case ItalicTextInline italic:
                        {
                            TextBlock tb = new TextBlock();
                            //tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownParagraph"));
                            tb.FontStyle = FontStyles.Italic;

                            foreach (var il in italic.Inlines)
                            {
                                ProcessInline(tb, il);
                            }

                            panel.Inlines.Add(tb);

                            break;
                        }

                    case MarkdownLinkInline linkInline:
                        {
                            Debug.WriteLine(linkInline.Url);



                            TextBlock buttonTextBlock = new TextBlock();
                            buttonTextBlock.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownParagraph"));
                            foreach (var il in linkInline.Inlines)
                            {
                                ProcessInline(buttonTextBlock, il);
                            }

                            Button but = new Button();
                            but.HorizontalAlignment = HorizontalAlignment.Left;



                            but.Content = buttonTextBlock;

                            if (!linkInline.Url.StartsWith("http"))
                            {
                                but.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownButton"));
                                but.Click += (object sender, RoutedEventArgs e) =>
                                {
                                    var asm = Assembly.GetAssembly(ViewModel.GetType());
                                    MethodInfo[] mi = ViewModel.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                                    Debug.Write("Executing " + linkInline.Url + " method");
                                    foreach (var t in mi)
                                    {
                                        Debug.WriteLine(t.Name.ToString());
                                    }
                                    MethodInfo thisOne = mi.First(x => x.Name == linkInline.Url);
                                    thisOne?.Invoke(ViewModel, null);
                                };
                            }
                            else
                            {
                                buttonTextBlock.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownHrefTextBlock"));
                                but.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownHref"));
                                but.Click += (object sender, RoutedEventArgs e) => NavigateToUrl(linkInline.Url);
                            }

                            panel.Inlines.Add(but);

                            break;
                        }

                    case ImageInline imageInline:
                        {
                            Image image = new Image();
                            image.Source = new BitmapImage(new Uri(imageInline.Url));
                            image.ToolTip = imageInline.Tooltip;
                            if (imageInline.ImageWidth > 0)
                            {
                                image.Width = imageInline.ImageWidth;
                            }

                            if (imageInline.ImageHeight > 0)
                            {
                                image.Height = imageInline.ImageHeight;
                            }

                            image.HorizontalAlignment = HorizontalAlignment.Left;
                            image.VerticalAlignment = VerticalAlignment.Top;

                            panel.Inlines.Add(image);

                            break;
                        }

                    case CodeInline codeInline:
                        {
                            TextBlock tb = new TextBlock();
                            tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownCode"));

                            tb.Text = codeInline.Text;

                            panel.Inlines.Add(tb);
                            break;
                        }
                    case InputInline inputInline:
                        {
                            switch (inputInline.InputType.ToLower())
                            {
                                case "text":
                                    {

                                        TextBox tb = new TextBox();
                                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                                        tb.Width = 300;
                                        tb.Text = GetVMValue(inputInline.BoundTo);



                                        if (RequireArgs(inputInline.Args, "width"))
                                        {
                                            tb.Width = int.Parse(inputInline.Args["width"]);
                                        }


                                        MarkDownViewModel vm = GetVM(inputInline.BoundTo);

                                        if (GetPropName(inputInline.BoundTo) != null && !vm.updateUIBindings.ContainsKey(GetPropName(inputInline.BoundTo)))
                                        {
                                            vm.updateUIBindings.Add(GetPropName(inputInline.BoundTo), new List<Action<string>>());
                                        }

                                        vm.updateUIBindings[GetPropName(inputInline.BoundTo)].Add(
                                            (newString) =>
                                            {
                                                tb.Text = newString;
                                            });

                                        tb.TextChanged += (object sender, TextChangedEventArgs e) =>
                                        {
                                            Type myType = vm.GetType();
                                            PropertyInfo myPropInfo = myType.GetProperty(GetPropName(inputInline.BoundTo));
                                            myPropInfo.SetValue(vm, tb.Text, null);
                                        };

                                        panel.Inlines.Add(tb);
                                        break;
                                    }

                                case "label":
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownParagraph"));

                                        tb.Text = GetVMValue(inputInline.BoundTo);

                                        if (RequireArgs(inputInline.Args, "width"))
                                        {
                                            tb.Width = int.Parse(inputInline.Args["width"]);
                                        }
                                        MarkDownViewModel vm = GetVM(inputInline.BoundTo);

                                        if (GetPropName(inputInline.BoundTo) != null && !vm.updateUIBindings.ContainsKey(GetPropName(inputInline.BoundTo)))
                                        {
                                            vm.updateUIBindings.Add(GetPropName(inputInline.BoundTo), new List<Action<string>>());
                                        }

                                        vm.updateUIBindings[GetPropName(inputInline.BoundTo)].Add(
                                            (newString) =>
                                            {
                                                tb.Text = newString;
                                            });


                                        panel.Inlines.Add(tb);
                                        break;
                                    }

                                case "slider":
                                    {

                                        Slider tb = new Slider();
                                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                                        tb.Width = 300;
                                        tb.Value = double.Parse(GetVMValue(inputInline.BoundTo));
                                        tb.Minimum = 0;
                                        tb.Maximum = 100;

                                        if (RequireArgs(inputInline.Args, "width")) tb.Width = int.Parse(inputInline.Args["width"]);
                                        if (RequireArgs(inputInline.Args, "min")) tb.Minimum = int.Parse(inputInline.Args["min"]);
                                        if (RequireArgs(inputInline.Args, "max")) tb.Minimum = int.Parse(inputInline.Args["max"]);


                                        MarkDownViewModel vm = GetVM(inputInline.BoundTo);

                                        if (GetPropName(inputInline.BoundTo) != null && !vm.updateUIBindings.ContainsKey(GetPropName(inputInline.BoundTo)))
                                        {
                                            vm.updateUIBindings.Add(GetPropName(inputInline.BoundTo), new List<Action<string>>());
                                        }

                                        vm.updateUIBindings[GetPropName(inputInline.BoundTo)].Add(
                                            (newString) =>
                                            {
                                                tb.Value = double.Parse(newString);
                                            });

                                        tb.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> e) =>
                                        {
                                            Type myType = vm.GetType();
                                            PropertyInfo myPropInfo = myType.GetProperty(GetPropName(inputInline.BoundTo));
                                            myPropInfo.SetValue(vm, tb.Value.ToString(), null);
                                        };

                                        panel.Inlines.Add(tb);
                                        break;
                                    }

                                case "dropdown":
                                    {
                                        if (RequireArgs(inputInline.Args, "source"))
                                        {
                                            ComboBox tb = new ComboBox();
                                            if (RequireArgs(inputInline.Args, "width")) tb.Width = int.Parse(inputInline.Args["width"]);
                                            Type myType = ViewModel.GetType();
                                            PropertyInfo myPropInfo = myType.GetProperty(inputInline.BoundTo);

                                            var tx = myPropInfo.GetValue(ViewModel);

                                            List<object> doop = (tx as IEnumerable<object>).Cast<object>().ToList();
                                            List<MarkDownViewModel> doop2 = (tx as IEnumerable<MarkDownViewModel>).Cast<MarkDownViewModel>().ToList();

                                            List<string> prps = new List<string>();
                                            string fname = inputInline.Args["source"];
                                            foreach (var op in doop2)
                                            {
                                                Type myType2 = op.GetType();
                                                PropertyInfo myPropInfo2 = myType2.GetProperty(fname);
                                                if (myPropInfo2 != null)
                                                {
                                                    var herp = myPropInfo2.GetValue(op);

                                                    Debug.WriteLine(herp);
                                                    prps.Add((string)herp);
                                                    tb.Items.Add(herp);
                                                }
                                            }



                                            if (RequireArgs(inputInline.Args, "selected"))
                                            {
                                                string selectedPropName = inputInline.Args["selected"];

                                                var tt = GetVMValue(selectedPropName);

                                                if (prps.Contains(tt))
                                                {
                                                    tb.SelectedIndex = prps.IndexOf(tt);
                                                }

                                                tb.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                                                {
                                                    int selected = tb.SelectedIndex;
                                                    string i1 = (string)tb.SelectedItem;
                                                    string i2 = prps[tb.SelectedIndex];
                                                    SetVMValue(selectedPropName, i1);
                                                };
                                            }


                                            panel.Inlines.Add(tb);
                                        }
                                        break;
                                    }
                            }

                            break;
                        }

                    case SuperscriptTextInline superScript:
                        {
                            Debug.WriteLine(superScript);

                            int tt = 0;
                            foreach (var i in superScript.Inlines)
                            {
                                Debug.WriteLine(tt + " : " + i.GetType() + "/" + i.ToString());
                                tt++;
                            }

                            if (superScript.Inlines.Count > 1 && superScript.Inlines.First() is CodeInline codeInline)
                            {
                                string cmd = codeInline.Text;
                                Dictionary<string, string> args = new Dictionary<string, string>();
                                for (int i = 1; i < superScript.Inlines.Count; i = i + 2)
                                {
                                    if (superScript.Inlines[i] is TextRunInline trl)
                                    {
                                        if (trl.Text.EndsWith("="))
                                        {
                                            if (superScript.Inlines.Count > i)
                                            {
                                                if (superScript.Inlines[i + 1] is CodeInline ci)
                                                {
                                                    args.Add(trl.Text.Substring(0, trl.Text.Length - 1).Trim(), ci.Text);
                                                }
                                            }
                                        }
                                    }
                                }

                                Debug.WriteLine(args);

                                switch (cmd.ToLower())
                                {

                                    case "input":
                                        {
                                            if (RequireArgs(args, "binding"))
                                            {

                                                TextBox tb = new TextBox();
                                                tb.HorizontalAlignment = HorizontalAlignment.Left;
                                                tb.Width = 300;
                                                tb.Text = GetVMValue(args["binding"]);

                                                if (RequireArgs(args, "width"))
                                                {
                                                    tb.Width = int.Parse(args["width"]);
                                                }


                                                MarkDownViewModel vm = GetVM(args["binding"]);

                                                if (!vm.updateUIBindings.ContainsKey(GetPropName(args["binding"])))
                                                {
                                                    vm.updateUIBindings.Add(GetPropName(args["binding"]), new List<Action<string>>());
                                                }

                                                vm.updateUIBindings[GetPropName(args["binding"])].Add(
                                                    (newString) =>
                                                    {
                                                        tb.Text = newString;
                                                    });

                                                tb.TextChanged += (object sender, TextChangedEventArgs e) =>
                                                {
                                                    Type myType = vm.GetType();
                                                    PropertyInfo myPropInfo = myType.GetProperty(GetPropName(args["binding"]));
                                                    myPropInfo.SetValue(vm, tb.Text, null);
                                                };

                                                panel.Inlines.Add(tb);

                                            }
                                            break;
                                        }

                                    case "slider":
                                        {
                                            if (RequireArgs(args, "binding"))
                                            {
                                                Slider sl = new Slider();
                                                sl.Minimum = 0;
                                                sl.Maximum = 100;
                                                sl.HorizontalAlignment = HorizontalAlignment.Left;
                                                sl.Width = 300;

                                                if (RequireArgs(args, "width"))
                                                {
                                                    sl.Width = int.Parse(args["width"]);
                                                }

                                                if (RequireArgs(args, "min"))
                                                {
                                                    sl.Minimum = int.Parse(args["min"]);
                                                }

                                                if (RequireArgs(args, "max"))
                                                {
                                                    sl.Maximum = int.Parse(args["max"]);
                                                }

                                                if (!ViewModel.updateUIBindings.ContainsKey(args["binding"]))
                                                {
                                                    ViewModel.updateUIBindings.Add(args["binding"], new List<Action<string>>());
                                                }

                                                ViewModel.updateUIBindings[args["binding"]].Add(
                                                    (newString) =>
                                                    {
                                                        int vl = 0;
                                                        if (int.TryParse(newString, out vl))
                                                        {
                                                            sl.Value = vl;
                                                        }
                                                    });

                                                sl.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> e) =>
                                                {
                                                    Type myType = ViewModel.GetType();
                                                    PropertyInfo myPropInfo = myType.GetProperty(args["binding"]);
                                                    myPropInfo.SetValue(ViewModel, sl.Value.ToString(), null);
                                                };

                                                panel.Inlines.Add(sl);
                                            }
                                            break;
                                        }
                                }
                            }

                            break;
                        }

                    //case CommentInline commentInline:
                    //    {
                    //        Debug.WriteLine(commentInline);
                    //        break;
                    //    }

                    default:

                        Debug.WriteLine("WTF IS '" + tp + "'");
                        break;
                }
            }
            catch { }
        }



        private bool RequireArgs(Dictionary<string, string> args, params string[] requiredArgs)
        {
            foreach (var s in requiredArgs)
            {
                if (!args.ContainsKey(s)) return false;
            }

            return true;
        }

        private void NavigateToUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }

        private void AddInlinesToStackPanel(StackPanel stackPanel, IList<Microsoft.Toolkit.Parsers.Markdown.Inlines.MarkdownInline> inlines)
        {
            TextBlock test = new TextBlock();
            test.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownParagraph"));


            foreach (var inline in inlines)
            {
                ProcessInline(test, inline);
            }

            stackPanel.Children.Add(test);
        }

        private void OldAddInlinesToStackPanel(StackPanel stackPanel, IList<Microsoft.Toolkit.Parsers.Markdown.Inlines.MarkdownInline> inlines, MarkdownInlineType style = MarkdownInlineType.TextRun)
        {
            foreach (var i2 in inlines)
            {
                MarkdownInlineType style2 = i2.Type;

                Debug.WriteLine("----------------------------------------------------------------------------------- " + style2);

                var lines = i2.ToString().Split('\n');
                foreach (var i in lines)
                {
                    StackPanel stack = new StackPanel();
                    stack.Orientation = Orientation.Horizontal;

                    string st2 = i.ToString().Replace("\r", "").Trim();

                    var tp = st2.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

                    foreach (var st in tp)
                    {
                        if (st.StartsWith("<<"))
                        {
                            string body = st.Substring(2).Trim();
                            string[] parts = body.Split(':');

                            string cmd = parts[1].Split('~').First();
                            List<string> prms = parts[1].Split('>').First().Split('~').ToList();
                            prms.RemoveAt(0);

                            switch (cmd.ToLower())
                            {
                                case "button":
                                    {
                                        Button but = new Button();
                                        but.HorizontalAlignment = HorizontalAlignment.Left;
                                        but.Content = prms[0];
                                        but.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownButton"));
                                        but.Click += (object sender, RoutedEventArgs e) =>
                                        {
                                            var asm = Assembly.GetAssembly(ViewModel.GetType());
                                            MethodInfo[] mi = ViewModel.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                                            MethodInfo thisOne = mi.First(x => x.Name.ToLower() == prms[1].ToLower());
                                            thisOne?.Invoke(ViewModel, null);
                                        };

                                        stack.Children.Add(but);
                                        break;
                                    }

                                case "text":
                                    {
                                        TextBox tb = new TextBox();
                                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                                        tb.Width = 300;
                                        tb.Text = GetVMValue(prms[0]);

                                        MarkDownViewModel vm = GetVM(prms[0]);

                                        if (!vm.updateUIBindings.ContainsKey(GetPropName(prms[0])))
                                        {
                                            vm.updateUIBindings.Add(GetPropName(prms[0]), new List<Action<string>>());
                                        }

                                        vm.updateUIBindings[GetPropName(prms[0])].Add(
                                            (newString) =>
                                            {
                                                tb.Text = newString;
                                            });

                                        tb.TextChanged += (object sender, TextChangedEventArgs e) =>
                                        {
                                            Type myType = vm.GetType();
                                            PropertyInfo myPropInfo = myType.GetProperty(GetPropName(prms[0]));
                                            myPropInfo.SetValue(vm, tb.Text, null);
                                        };
                                        stack.Children.Add(tb);
                                        break;
                                    }

                                case "slider":
                                    {
                                        Slider sl = new Slider();
                                        sl.Minimum = int.Parse(prms[1]);
                                        sl.Maximum = int.Parse(prms[2]);
                                        sl.HorizontalAlignment = HorizontalAlignment.Left;
                                        sl.Width = 300;

                                        if (!ViewModel.updateUIBindings.ContainsKey(prms[0]))
                                        {
                                            ViewModel.updateUIBindings.Add(prms[0], new List<Action<string>>());
                                        }

                                        ViewModel.updateUIBindings[prms[0]].Add(
                                            (newString) =>
                                            {
                                                int vl = 0;
                                                if (int.TryParse(newString, out vl))
                                                {
                                                    sl.Value = vl;
                                                }
                                            });

                                        sl.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> e) =>
                                        {
                                            Type myType = ViewModel.GetType();
                                            PropertyInfo myPropInfo = myType.GetProperty(prms[0]);
                                            myPropInfo.SetValue(ViewModel, sl.Value.ToString(), null);
                                        };

                                        stack.Children.Add(sl);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            TextBlock tb = new TextBlock();
                            tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownParagraph"));

                            tb.Text = String.Join(" ", i);


                            if (style == MarkdownInlineType.Bold)
                            {
                                tb.FontWeight = FontWeights.ExtraBold;
                            }

                            if (style == MarkdownInlineType.Italic)
                            {
                                tb.FontStyle = FontStyles.Italic;
                            }

                            stack.Children.Add(tb);
                        }
                    }

                    stackPanel.Children.Add(stack);
                }
            }
        }



        private string GetVMValue(string propName)
        {
            if (propName.StartsWith("["))
            {
                string pp = propName.Substring(1, propName.Length - 2);
                string pname = pp.Substring(0, pp.IndexOf("["));

                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(pname);
                object derp = myPropInfo.GetValue(ViewModel);

                string index = pp.Substring(pp.IndexOf("[") + 1);
                index = index.Substring(0, index.IndexOf("]"));


                List<object> doop2 = (derp as IEnumerable<object>).Cast<object>().ToList();

                object doop3 = doop2[int.Parse(index)];

                string subprobName = pp.Substring(pp.IndexOf(".") + 1);

                Type myType2 = doop3.GetType();
                PropertyInfo myPropInfo2 = myType2.GetProperty(subprobName);
                object derpy = myPropInfo2.GetValue(doop3);

                return derpy.ToString();

                return null;
            }
            else
            {
                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propName);
                if (myPropInfo == null) return null;
                object derp = myPropInfo.GetValue(ViewModel);
                return derp.ToString();
            }
        }

        private string GetPropName(string propName)
        {
            if (propName.StartsWith("["))
            {
                string pp = propName.Substring(1, propName.Length - 2);
                string subprobName = pp.Substring(pp.IndexOf(".") + 1);
                return subprobName;
            }
            else
            {
                return propName;
            }
        }

        private MarkDownViewModel GetVM(string propName)
        {
            if (propName.StartsWith("["))
            {
                string pp = propName.Substring(1, propName.Length - 2);
                string pname = pp.Substring(0, pp.IndexOf("["));

                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(pname);
                object derp = myPropInfo.GetValue(ViewModel);

                string index = pp.Substring(pp.IndexOf("[") + 1);
                index = index.Substring(0, index.IndexOf("]"));


                List<object> doop2 = (derp as IEnumerable<object>).Cast<object>().ToList();

                object doop3 = doop2[int.Parse(index)];

                string subprobName = pp.Substring(pp.IndexOf(".") + 1);

                return doop3 as MarkDownViewModel;

                Type myType2 = doop3.GetType();
                PropertyInfo myPropInfo2 = myType2.GetProperty(subprobName);
                object derpy = myPropInfo2.GetValue(doop3);

                return derpy as MarkDownViewModel;

                return null;
            }
            else
            {
                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propName);
                if (myPropInfo == null) return null;
                object derp = myPropInfo.GetValue(ViewModel);
                var herp = derp as MarkDownViewModel;

                if (herp == null)
                {
                    return ViewModel;
                }
                else
                {
                    return herp;
                }
            }
        }

        private void SetVMValue(string propName, string value)
        {
            if (propName.StartsWith("["))
            {
                string pp = propName.Substring(1, propName.Length - 2);
                string pname = pp.Substring(0, pp.IndexOf("["));

                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(pname);
                object derp = myPropInfo.GetValue(ViewModel);

                string index = pp.Substring(pp.IndexOf("[") + 1);
                index = index.Substring(0, index.IndexOf("]"));


                List<object> doop2 = (derp as IEnumerable<object>).Cast<object>().ToList();

                object doop3 = doop2[int.Parse(index)];

                string subprobName = pp.Substring(pp.IndexOf(".") + 1);

                Type myType2 = doop3.GetType();
                PropertyInfo myPropInfo2 = myType2.GetProperty(subprobName);
                myPropInfo2.SetValue(doop3, value);

            }
            else
            {
                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propName);
                myPropInfo.SetValue(ViewModel, value);
            }
        }
        PropertyInfo GetProp(string propName)
        {
            if (propName.StartsWith("["))
            {
                string pp = propName.Substring(1, propName.Length - 2);
                string pname = pp.Substring(0, pp.IndexOf("["));

                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(pname);
                object derp = myPropInfo.GetValue(ViewModel);

                string index = pp.Substring(pp.IndexOf("[") + 1);
                index = index.Substring(0, index.IndexOf("]"));


                List<object> doop2 = (derp as IEnumerable<object>).Cast<object>().ToList();

                object doop3 = doop2[int.Parse(index)];

                string subprobName = pp.Substring(pp.IndexOf(".") + 1);

                Type myType2 = doop3.GetType();
                PropertyInfo myPropInfo2 = myType2.GetProperty(subprobName);

                return myPropInfo2;
            }
            else
            {
                Type myType = ViewModel.GetType();
                PropertyInfo myPropInfo = myType.GetProperty(propName);

                return myPropInfo;
            }
        }
        Style GetResource(string name)
        {
            object found = resourceDictionary[name];
            return (Style)found;
        }
        public string Unroll(string markdown)
        {
            var thing = Regex.Matches(markdown, "<<LoopOver~(.+)>>");

            List<string> finders = new List<string>();

            foreach (Match m in thing)
            {
                string p = m.Value.Substring(11, m.Value.Length - 13);

                var otherThing = markdown.IndexOf("<<LoopEnd~" + p + ">");
                if (otherThing >= 0)
                {

                    var thing2 = Regex.Matches(markdown, "(?s)(?<=<<LoopOver~" + p + ">>).*?(?=<<LoopEnd~" + p + ">>)");
                    if (thing2.Count > 0)
                    {
                        var tttt = thing2[0];

                        Type myType = ViewModel.GetType();
                        PropertyInfo myPropInfo = myType.GetProperty(p);

                        var tx = myPropInfo.GetValue(ViewModel);

                        List<object> doop = (tx as IEnumerable<object>).Cast<object>().ToList();
                        List<MarkDownViewModel> doop2 = (tx as IEnumerable<MarkDownViewModel>).Cast<MarkDownViewModel>().ToList();

                        string newbody = "";
                        for (int i = 0; i < doop2.Count; i++)
                        {
                            string item = tttt.Value.TrimStart().Replace(p, p + "[" + i + "]");
                            newbody = newbody + item;
                        }

                        markdown = markdown.Replace($"<<LoopOver~{p}>>{tttt.Value}<<LoopEnd~{p}>>", newbody);

                        //Debug.WriteLine(tx);
                    }

                }
                finders.Add(p);
            }

            Debug.WriteLine(markdown);
            return markdown;
        }

        public void RenderToUI(StackPanel stackPanel, [CallerMemberName] string callerMemberName = "")
        {
            stackPanel.Children.Clear();

            string tmp = Markdown;
            var tt = Unroll(tmp);

            MarkdownDocument document = new MarkdownDocument();

            document.Parse(tt);

            RenderBlocks(stackPanel, document.Blocks);
        }

        void RenderBlocks(StackPanel stackPanel, IList<MarkdownBlock> blocks)
        {

            foreach (MarkdownBlock b in blocks)
            {
                //Debug.WriteLine(b.GetType() + " - " + b.Type);

                switch (b)
                {
                    case HorizontalRuleBlock horizontalRuleBlock:
                        {
                            Grid gd = new Grid();
                            gd.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownHorizontalRule"));

                            stackPanel.Children.Add(gd);

                            break;
                        }

                    case ListBlock listBlock:
                        {
                            Grid gd = new Grid();

                            gd.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownOrderedList"));

                            gd.ColumnDefinitions.Add(new ColumnDefinition());
                            gd.ColumnDefinitions.Add(new ColumnDefinition());

                            bool numbered = listBlock.Style == ListStyle.Numbered;

                            int ay = -1;
                            foreach (var l in listBlock.Items)
                            {
                                gd.RowDefinitions.Add(new RowDefinition());
                                ay++;

                                TextBlock tbx = new TextBlock();
                                tbx.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownOrderedListIndex"));


                                if (numbered)
                                {
                                    tbx.Text = (ay + 1).ToString() + ".";
                                }
                                else
                                {
                                    tbx.Text = "•";
                                }

                                gd.Children.Add(tbx);
                                Grid.SetColumn(tbx, 0);
                                Grid.SetRow(tbx, ay);


                                StackPanel sspItem = new StackPanel();
                                RenderBlocks(sspItem, l.Blocks);

                                Grid.SetColumn(sspItem, 1);
                                Grid.SetRow(sspItem, ay);
                                gd.Children.Add(sspItem);






                            }

                            stackPanel.Children.Add(gd);
                            break;
                        }
                    case QuoteBlock quoteBlock:
                        {
                            Border brd = new Border();
                            brd.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownBlockQuoteBorder"));

                            Grid grd = new Grid();
                            grd.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownBlockQuoteGrid"));

                            brd.Child = grd;
                            StackPanel ssp = new StackPanel();

                            RenderBlocks(ssp, quoteBlock.Blocks);

                            grd.Children.Add(ssp);

                            stackPanel.Children.Add(brd);
                            break;
                        }

                    case HeaderBlock headerblock:
                        {
                            TextBlock tb = new TextBlock();
                            tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownHeader" + headerblock.HeaderLevel));
                            int fontSize = (5 - headerblock.HeaderLevel) * 12;
                            //tb.FontSize = fontSize;
                            tb.Text = String.Join("\r\n", headerblock.Inlines);

                            stackPanel.Children.Add(tb);
                            break;
                        }

                    case ParagraphBlock paragraph:
                        {
                            //Debug.WriteLine(paragraph);

                            AddInlinesToStackPanel(stackPanel, paragraph.Inlines);

                            break;



                        }

                    case TableBlock tableBlock:
                        {
                            Grid grid = new Grid();

                            foreach (var t in tableBlock.Rows)
                            {
                                grid.RowDefinitions.Add(new RowDefinition());
                            }

                            foreach (var t in tableBlock.ColumnDefinitions)
                            {
                                grid.ColumnDefinitions.Add(new ColumnDefinition());
                            }

                            int ay = 0;
                            foreach (var y in tableBlock.Rows)
                            {
                                int ax = 0;
                                foreach (var x in tableBlock.ColumnDefinitions)
                                {
                                    if (y.Cells.Count >= ax)
                                    {
                                        try
                                        {
                                            StackPanel sp = new StackPanel();
                                            Border border = new Border();
                                            border.BorderBrush = Brushes.Black;
                                            border.BorderThickness = new Thickness(1);
                                            border.Child = sp;
                                            grid.Children.Add(border);

                                            Grid.SetColumn(border, ax);
                                            Grid.SetRow(border, ay);

                                            var current = y.Cells[ax];

                                            AddInlinesToStackPanel(sp, current.Inlines);
                                        }
                                        catch { }
                                        ax++;
                                    }
                                }

                                ay++;
                            }

                            stackPanel.Children.Add(grid);

                            break;
                        }

                    case CodeBlock codeBlock:
                        {
                            Grid codeBlockGrid = new Grid();
                            codeBlockGrid.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownCodeGrid"));

                            TextBlock tb = new TextBlock();
                            tb.SetValue(FrameworkElement.StyleProperty, GetResource("MarkdownCodeBlock"));

                            tb.Text = codeBlock.Text;

                            codeBlockGrid.Children.Add(tb);
                            stackPanel.Children.Add(codeBlockGrid);

                            break;
                        }

                    default:
                        {
                            var unknownType = b.GetType();

                            Debug.WriteLine("Cant handle " + unknownType);
                            break;
                        }
                }
            }
        }


    }
}
