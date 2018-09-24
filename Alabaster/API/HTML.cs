using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Alabaster
{
    public abstract class HTMLBase
    {
        internal HTMLBase() { }
    }

    public class HTML : HTMLBase
    {
        private ConcurrentDictionary<string, List<Tag>> tagsByClass = new ConcurrentDictionary<string, List<Tag>>(Environment.ProcessorCount, 100);
        private ConcurrentDictionary<string, List<Tag>> tagsByTagName = new ConcurrentDictionary<string, List<Tag>>(Environment.ProcessorCount, 100);
        private ConcurrentDictionary<string, Tag> tagsByID = new ConcurrentDictionary<string, Tag>(Environment.ProcessorCount, 100);
        private ConcurrentDictionary<string, bool> MaskedAttributes = new ConcurrentDictionary<string, bool>(Environment.ProcessorCount, 100);

        public Tag RootElement { get; protected internal set; }
        public Tag Body { get; protected internal set; }

        internal HTML() => this.RootElement = new Tag(this);
        public static HTML FromHTMLFile(string file) => ParseHTML(Encoding.UTF8.GetString(FileIO.GetFile(file).Data));
        public static HTML FromString(string str) => ParseHTML(str);

        public HTML Copy()
        {
            HTML newHtml = new HTML();
            foreach (Node child in this.RootElement.Children)
            {
                newHtml.RootElement.AddChild(child.GetCopy());
            }
            return newHtml;
        }

        public Tag GetElementByID(string id) => (id != null && tagsByID.TryGetValue(id, out Tag el)) ? ((el.Document == this) ? el : null) : null;
        public Tag[] GetElementsByClassName(string className) => tagsByClass.TryGetValue(className, out List<Tag> tags) ? tags.FindAll((Tag t) => t.Document == this).ToArray() : new Tag[] { };
        public Tag[] GetElementsByTagName(string tagName) => tagsByTagName.TryGetValue(tagName, out List<Tag> tags) ? tags.FindAll((Tag t) => t.Document == this).ToArray() : new Tag[] { };

        public void MaskAttributes(params string[] attributes) => Array.ForEach(attributes, (string toMask) => this.MaskedAttributes.TryAdd(toMask, true));
        public void UnmaskAttributes(params string[] attributes) => Array.ForEach(attributes, (string toUnmask) => this.MaskedAttributes.TryUpdate(toUnmask, false, true));
        public bool IsAttributeMasked(string attribute) => this.MaskedAttributes.TryGetValue(attribute, out bool result) ? result : false;

        public string Render() => this.Render(false);
        public string Render(bool pretty, int indentSize = 4)
        {
            StringBuilder sb = new StringBuilder();
            AddNodes(this.RootElement.GetChildren());
            return sb.ToString();

            void AddNodes(Node[] nodes, int depth = 0)
            {
                string indent = (pretty) ? new string(' ', indentSize) : null;
                foreach (Node node in nodes)
                {
                    if (pretty) { for (int i = 0; i < depth; i++) { sb.Append(indent); } }
                    sb.Append(node.GetOpenTag());
                    if (pretty) { sb.Append("\n"); }
                    sb.Append(node.GetContentBody());
                    AddNodes(node.GetChildren(), depth + 1);
                    sb.Append(node.GetCloseTag());
                    if (pretty) { sb.Append("\n"); }
                }
            }
        }

        internal static HTML ParseHTML(string content)
        {
            HTML doc = new HTML();
            int position = 0;
            int currentSubstringStart = 0;
            Tag currentTag = doc.RootElement;
            Action Next = ParsingTagContent;

            while (position < content.Length && Next != null)
            {
                Next();
                position++;
            }

            return doc;

            void ParsingTagContent()
            {
                if (CurrentChar() == '<')
                {
                    if (NextChar() == '/') { Next = ParsingClosingTag; }
                    else if (isHTMLAlphaNumeric(NextChar()))
                    {
                        TextNode text = new TextNode(CurrentSubstring());
                        currentTag.AddChild(text);
                        currentSubstringStart = position + 1;
                        Next = ParsingTagName;
                    }
                    else if (content.Substring(position, 4) == "<!--") { Next = ParsingComment; }
                }
            }

            void ParsingComment()
            {
                if (content.Substring(position, 3) == "-->") { Next = ParsingTagContent; }
            }

            void ParsingClosingTag()
            {
                Tag t = currentTag;
                while (t != null)
                {
                    string expected = string.Join(null, "</", t.Name, ">");
                    string actual = content.Substring(position - 1, expected.Length);
                    if (actual == expected)
                    {
                        currentTag = t.Parent;
                        Next = ParsingTagContent;
                        return;
                    }
                    else { t = t.Parent; }
                }
            }

            void ParsingTagName()
            {
                if (isHTMLWhiteSpace(CurrentChar()))
                {
                    Tag t = new Tag(CurrentSubstring());
                    currentTag.AddChild(t);
                    currentTag = t;
                    Next = ParsingTagAttributes;
                }
            }

            void ParsingTagAttributes()
            {
                if (CurrentChar() == '"' || CurrentChar() == '\'') { position = content.IndexOf(CurrentChar(), position + 1) + 1; }
                else if (CurrentChar() == '>')
                {
                    currentTag.AddAttributes(CurrentSubstring());
                    currentSubstringStart = position + 1;
                    currentTag = currentTag.Parent;
                    Next = ParsingTagContent;
                }
            }

            char CurrentChar() => GetChar(position);
            char NextChar() => GetChar(position + 1);
            char GetChar(int index) => (index < content.Length) ? content[index] : '\0';
            string CurrentSubstring() => (currentSubstringStart < position) ? content.Substring(currentSubstringStart, position - currentSubstringStart) : "";
        }

        internal static bool isHTMLAlphaNumeric(char c) => Char.IsLetterOrDigit(c) && c < 255;
        internal const string WhiteSpaceChars = "\u0009\u000A\u000C\u000D\u0020";
        internal static bool isHTMLWhiteSpace(char c) => WhiteSpaceChars.IndexOf(c) != -1;

        internal void AddTagWithID(Tag t, string id) => this.tagsByID[id] = t;
        internal void AddTagWithClass(Tag t, string c) => this.AddToListDict(this.tagsByClass, t, c);
        internal void AddTagWithTagName(Tag t, string n) => this.AddToListDict(this.tagsByTagName, t, n);
        private void AddToListDict(ConcurrentDictionary<string, List<Tag>> dict, Tag t, string k)
        {
            if (!dict.TryGetValue(k, out List<Tag> tags))
            {
                tags = new List<Tag>(5);
                this.tagsByClass[k] = tags;
            }
            if (!tags.Contains(t)) { tags.Add(t); }
        }

        internal void RemoveTagWithID(string id) => tagsByID.TryRemove(id, out _);
        internal void RemoveTagWithClass(Tag t, string c) => this.RemoveFromListDict(this.tagsByClass, t, c);
        internal void RemoveTagWithTagName(Tag t, string n) => this.RemoveFromListDict(this.tagsByTagName, t, n);
        private bool RemoveFromListDict(ConcurrentDictionary<string, List<Tag>> dict, Tag t, string k) => dict.TryGetValue(k, out List<Tag> tags) ? tags.Remove(t) : false;        
    }

    public abstract class Node : HTMLBase
    {
        internal Node() { }
        internal abstract Node[] GetChildren();
        internal abstract string GetOpenTag();
        internal abstract string GetCloseTag();
        internal abstract string GetContentBody();
        internal abstract Node GetCopy();
        internal abstract string GetTagName();
    }

    public sealed class TextNode : Node
    {
        private string content;
        public string Content
        {
            get => this.content;
            set => Interlocked.Exchange(ref this.content, value);
        }
        public TextNode(string content) => this.Content = content;
        public static explicit operator TextNode(string content) => new TextNode(content);

        internal override Node[] GetChildren() => new Node[] { };
        internal override Node GetCopy() => new TextNode(this.content);
        internal override string GetOpenTag() => string.Empty;
        internal override string GetCloseTag() => string.Empty;
        internal override string GetContentBody() => this.content;
        internal override string GetTagName() => string.Empty;
    }
        
    public sealed class Tag : Node
    {
        private static readonly ConcurrentDictionary<(Tag tag, TagAttributeKey key), string> tagAttributes = new ConcurrentDictionary<(Tag, TagAttributeKey), string>(Environment.ProcessorCount, 100);

        //actual data members
        public string Name;
        private HTMLBase _parent;
        private List<Node> _children;
        //
            
        public Node[] Children => this._children?.ToArray() ?? new Node[] { };
        public Tag Parent
        {
            get
            {
                if(this._parent is Tag) { return this._parent as Tag; }
                else { return null; }
            }
            private set => this._parent = value;
        }
        public HTML Document { get => (this.Parent != null) ? this.Parent.Document : this._parent as HTML; }
        public bool IsImmutable { get => this.Document is ImmutableHTML; }
        public AttributeCollection Attributes
        {
            get
            {
                List<(string, string)> attribs = new List<(string, string)>(10);
                foreach((Tag tag, TagAttributeKey key) in tagAttributes.Keys)
                {
                    if(tag == this) { attribs.Add((key.Value, tagAttributes[(this, key)])); }
                }
                return new AttributeCollection(attribs.ToArray());
            }
        }

        public AttributeCollection MaskedAttributes
        {
            get
            {
                AttributeCollection attributes = this.Attributes;
                HTML doc = this.Document as HTML;
                List<(string, string)> masked = new List<(string, string)>(attributes.Length);
                for (int i = 0; i < attributes.Length; i++)
                {
                    (string key, string value) attr = attributes[i];
                    if (doc.IsAttributeMasked(attr.key)) { masked.Add(attr); }
                }
                return new AttributeCollection(masked.ToArray());
            }
        }

        public AttributeCollection UnmaskedAttributes
        {
            get
            {
                AttributeCollection attributes = this.Attributes;
                HTML doc = this.Document as HTML;
                List<(string, string)> unmasked = new List<(string, string)>(attributes.Length);
                for (int i = 0; i < attributes.Length; i++)
                {
                    (string key, string value) attr = attributes[i];
                    if (!doc.IsAttributeMasked(attr.key)) { unmasked.Add(attr); }
                }
                return new AttributeCollection(unmasked.ToArray());
            }
        }

        internal Tag(HTML parent) : this("", "") { this._parent = parent; }
        public Tag(string name) : this(name, "") { }
        public Tag(string name, string attributes)
        {
            this.Name = string.Intern(name.ToLower());
            this.AddAttributes(attributes);
        }
        private Tag(Tag original)
        {
            this.Name = original.Name;
            this._parent = original._parent;
            if (original._children != null)
            {
                this._children =  new List<Node>(original._children.Capacity);
                foreach (Node child in this._children)
                {
                    this._children.Add(child.GetCopy());
                }
            }
        }
        public static explicit operator Tag(string name) => new Tag(name);

        public string this[string AttributeName]
        {
            get => tagAttributes.TryGetValue((this, AttributeName), out string val) ? val.Trim('\'', '\"') : null;
            set
            {
                if(this.IsImmutable) { return; }
                TagAttributeKey key = AttributeName;
                HTML doc = this.Document;
                switch(key.Value)
                {
                    case "id":
                        doc.RemoveTagWithID(this["id"]);
                        doc.AddTagWithID(this, value);
                        break;
                    case "class":
                        string[] classes = value.Trim('\'', '\"').Split(HTML.WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] oldClasses = this["class"].Trim('\'', '\"').Split(HTML.WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string className in oldClasses)
                        {
                            doc.RemoveTagWithClass(this, className);
                        }
                        foreach (string className in classes)
                        {
                            doc.AddTagWithClass(this, className);
                        }
                        break;                            
                }
                tagAttributes[(this, AttributeName)] = value;                    
            }
        }

        public Node Copy() => new Tag(this);

        //internal Node class API
        internal override Node[] GetChildren() => this.Children;
        internal override Node GetCopy() => this.Copy();
        internal override string GetOpenTag() => string.Join(null, "<", this.Name, this.UnmaskedAttributes.ToString(), ">");
        internal override string GetCloseTag() => string.Join(null, "</", this.Name, ">");
        internal override string GetContentBody() => string.Empty;
        internal override string GetTagName() => this.Name;
        //

        internal void SwitchParent(ImmutableHTML newParent) => this._parent = newParent;
              
        public void AddAttributes(string attributes)
        {
            if(this.IsImmutable || string.IsNullOrWhiteSpace(attributes)) { return; }
            if(attributes.IndexOf('\0') != -1) { attributes = string.Join(null, attributes.Split('\0')); }
            string[] tokens = GetTokens();
            int position = 0;
            while(position < tokens.Length)
            {
                if(position < (tokens.Length - 2) && tokens[position + 1] == "=")
                {
                    this[tokens[position]] = tokens[position + 2];
                    position += 2;
                }
                else
                {
                    this[tokens[position]] = null;
                    position++;
                }
            }

            string[] GetTokens()
            {
                char[] rawChars = attributes.ToCharArray();
                Queue<string> strLiterals = new Queue<string>(20);
                char quote = '\0';
                int strStartIndex = 0;
                for(int i = 0; i < attributes.Length; i++)
                {
                    char ch = attributes[i];
                    if(quote != '\0')
                    {
                        if(ch == quote)
                        {
                            int end = i - strStartIndex;
                            strLiterals.Enqueue(attributes.Substring(strStartIndex, end));                                
                            for(int j = strStartIndex; j < end; j++)
                            {
                                rawChars[j] = '\0';
                            }
                            quote = '\0';
                        }
                    }
                    else if(ch == '\"' || ch == '\'')
                    {
                        quote = ch;
                        strStartIndex = i;
                    }
                }
                string[] results = rawChars.ToString().Split(HTML.WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < results.Length; i++)
                {
                    if(results[i][0] == '\0') { results[i] = strLiterals.Dequeue(); }
                }
                return results;
            }
        }

        public Tag AppendTextNode(string text) => (this.IsImmutable) ? null : this.AddChild(new TextNode(text));
        public Tag AddChild(Node child) => (child is Tag) ? this.AddChild(child as Tag) : (child is TextNode) ? this.AddChild(child as TextNode) : null;
        public Tag AddChild(TextNode text) => (this.IsImmutable) ? null : this.AddInternal(text);
        public Tag AddChild(Tag child)
        {
            if(this.IsImmutable || this.Contains(child)) { return null; }
            child.Parent?.RemoveChild(child);
            child.Parent = this;
            this.AddInternal(child);
            HTML doc = this.Document;
            if(doc != null && doc.Body == null && child.Name == "body") { doc.Body = child; }
            doc.AddTagWithTagName(this, this.Name);
            string[] classes = this["class"].Trim('\'', '\"').Split(HTML.WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach(string c in classes)
            {
                doc.AddTagWithClass(this, c);
            }
            doc.AddTagWithID(this, this["id"]);
            return this;
        }

        private Tag AddInternal(Node child)
        {
            if(this._children == null) { this._children = new List<Node>(5); }
            this._children.Add(child);
            return this;
        }

        public void AddChildren(params Tag[] children) => Array.ForEach(children, (Tag child) => this.AddChild(child));
        public bool RemoveChild(Tag child) => (this.IsImmutable || this._children == null) ? false : this._children.Remove(child);            

        public bool Contains(Tag tag)
        {
            Tag parent = tag.Parent;
            while(parent != null)
            {
                if(parent == this) { return true; }
                parent = parent.Parent;
            }
            return false;
        }
                        
        private struct TagAttributeKey
        {
            public readonly string Value;
            public TagAttributeKey(string key) => this.Value = key.Trim('\"', '\'').ToLower();
            public static implicit operator TagAttributeKey(string key) => new TagAttributeKey(key);
        }

        public readonly struct AttributeCollection
        {
            private readonly (string Key, string Value)[] pairs;
            public (string Key, string Value) this[int index] => this.pairs[index];
            public int Length => this.pairs.Length;

            public override string ToString()
            {
                string[] arr = new string[pairs.Length * 5];
                for(int i = 0; i < this.pairs.Length; i++)
                {
                    (string key, string value) = this.pairs[i];
                    int arrIndex = i * 5;
                    arr[arrIndex] = " ";
                    arr[arrIndex + 1] = key;
                    if(!string.IsNullOrEmpty(value))
                    {
                        arr[arrIndex + 2] = "=";
                        arr[arrIndex + 3] = value;
                        arr[arrIndex + 4] = " ";
                    }
                    else { arr[arrIndex + 2] = " "; }
                }
                return string.Join(null, arr);
            }
            internal AttributeCollection((string key, string value)[] attributes) => this.pairs = attributes;
        }
    }    

    public sealed class ImmutableHTML : HTML
    {
        internal ImmutableHTML() { }
        private ImmutableHTML(FileIO.FileData file) => this.RootElement = HTML.FromHTMLFile(Encoding.UTF8.GetString(file.Data)).RootElement;
        private ImmutableHTML(string content)
        {
            Tag root = HTML.FromString(content).RootElement;
            root.SwitchParent(this);
            this.RootElement = root;
        }
        public static new ImmutableHTML FromHTMLFile(string file) => new ImmutableHTML(FileIO.GetFile(file));
        public static new ImmutableHTML FromString(string str) => new ImmutableHTML(str);
        private static ImmutableHTML _template;
        public static ImmutableHTML EmptyTemplate
        {
            get
            {
                Interlocked.CompareExchange(ref _template, new ImmutableHTML("<!DOCTYPE html><html><head><title></title></head><body></body></html>"), null);
                return _template;
            }
        }
    }
}