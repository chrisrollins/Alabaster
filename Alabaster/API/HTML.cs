using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Alabaster
{
    public abstract class HTMLBase
    {
        internal HTMLBase() { }
    }

    public class HTML : HTMLBase
    {
        internal ConcurrentDictionary<string, List<Tag>> tagsByClass = new ConcurrentDictionary<string, List<Tag>>(Environment.ProcessorCount, 100);
        internal ConcurrentDictionary<string, List<Tag>> tagsByTagName = new ConcurrentDictionary<string, List<Tag>>(Environment.ProcessorCount, 100);
        internal ConcurrentDictionary<string, Tag> tagsByID = new ConcurrentDictionary<string, Tag>(Environment.ProcessorCount, 100);
        public Tag RootElement { get; protected internal set; }
        internal HTML() => this.RootElement = new Tag(this);
        public static HTML FromHTMLFile(string file) => new HTML().ParseHTML(Encoding.UTF8.GetString(FileIO.GetFile(file).Data));
        public static HTML FromString(string str) => new HTML().ParseHTML(str);
        
        public HTML Copy()
        {
            HTML newHtml = new HTML();
            foreach(Tag child in this.RootElement.Children)
            {
                newHtml.RootElement.AddChild(child.Copy());
            }
            return newHtml;
        }

        public Tag GetElementByID(string id) => (id != null && tagsByID.TryGetValue(id, out Tag el)) ? ((el.Document == this) ? el : null) : null;
        public Tag[] GetElementsByClassName(string className) => tagsByClass.TryGetValue(className, out List<Tag> tags) ? tags.FindAll((Tag t) => t.Document == this).ToArray() : new Tag[] { };
        public Tag[] GetElementsByTagName(string tagName) => tagsByTagName.TryGetValue(tagName, out List<Tag> tags) ? tags.FindAll((Tag t) => t.Document == this).ToArray() : new Tag[] { };        
        
        public string Render()
        {
            StringBuilder sb = new StringBuilder();
            AddTag(this.RootElement);
            return sb.ToString();

            void AddTag(Tag tag)
            {
                if(tag != this.RootElement)
                {
                    if (!string.IsNullOrEmpty(tag.Name)) { sb.Append("<").Append(tag.Name).Append(tag.Attributes.ToString()).Append(">"); }
                    else { sb.Append(tag.Content); }
                }
                foreach(Tag child in tag.Children)
                {
                    AddTag(child);
                }
                if (!string.IsNullOrEmpty(tag.Name)) { sb.Append("</").Append(tag.Name).Append(">"); }
            }
        }

        internal HTML ParseHTML(string content)
        {
            int position = 0;
            int currentSubstringStart = 0;
            Tag currentTag = this.RootElement;
            Action Next = ParsingTagContent;

            while(position < content.Length && Next != null)
            {
                Next();
                position++;
            }

            return this;
            
            void ParsingTagContent()
            {
                if(CurrentChar() == '<')
                {
                    if (NextChar() == '/') { Next = ParsingClosingTag; }
                    else if (isHTMLAlphaNumeric(NextChar()))
                    {
                        Tag text = new Tag((string)null);
                        text.Content = CurrentSubstring();
                        currentTag.AddChild(text);
                        currentSubstringStart = position + 1;
                        Next = ParsingTagName;
                    }
                    else if(content.Substring(position, 4) == "<!--") { Next = ParsingComment; }
                }
            }

            void ParsingComment()
            {
                if(content.Substring(position, 3) == "-->") { Next = ParsingTagContent; }
            }

            void ParsingClosingTag()
            {
                Tag t = currentTag;
                while(t != null)
                {
                    string expected = String.Join(null, "</", t.Name, ">");
                    string actual = content.Substring(position - 1, expected.Length);
                    if(actual == expected)
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
                    Tag t = (Tag)CurrentSubstring();
                    currentTag.AddChild(t);
                    currentTag = t;
                    Next = ParsingTagAttributes;
                }
            }

            void ParsingTagAttributes()
            {
                if(CurrentChar() == '"' || CurrentChar() == '\'') { position = content.IndexOf(CurrentChar(), position + 1) + 1; }
                else if(CurrentChar() == '>')
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

        private static bool isHTMLAlphaNumeric(char c) => Char.IsLetterOrDigit(c) && c < 255;

        private const string WhiteSpaceChars = "\u0009\u000A\u000C\u000D\u0020";
        private static bool isHTMLWhiteSpace(char c) => WhiteSpaceChars.IndexOf(c) != -1;        

        public sealed class Tag : HTMLBase
        {
            private static readonly ConcurrentDictionary<(Tag tag, TagAttributeKey key), string> tagAttributes = new ConcurrentDictionary<(Tag, TagAttributeKey), string>(Environment.ProcessorCount, 100);
            public string Name;
            public string Content = "";
            private HTMLBase parent;
            public Tag Parent
            {
                get
                {
                    if(this.parent is Tag) { return this.parent as Tag; }
                    else { return null; }
                }
                private set => this.parent = value;
                
            }
            public HTMLBase Document { get => (this.Parent != null) ? this.Parent.Document : this.parent; }
            public bool IsImmutable { get => this.Document is ImmutableHTML; }
            private readonly List<Tag> children;
            public Tag[] Children => children.ToArray();
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

            internal Tag(HTML parent) : this("", "") { this.parent = parent; }
            public Tag(string name) : this(name, "") { }
            public Tag(string name, string attributes)
            {
                this.Name = name.ToLower();
                this.AddAttributes(attributes);
            }
            private Tag(Tag original)
            {
                this.Name = original.Name;
                this.parent = original.parent;
                this.Content = original.Content;
                this.children = new List<Tag>(this.children.Capacity);
                foreach(Tag child in this.children)
                {
                    this.children.Add(child.Copy());
                }
            }
            public static explicit operator Tag(string name) => new Tag(name);

            public string this[string AttributeName]
            {
                get => tagAttributes.TryGetValue((this, AttributeName), out string val) ? val : null;
                set
                {
                    if (!this.IsImmutable)
                    {
                        TagAttributeKey key = AttributeName;
                        HTML doc = this.Document as HTML;
                        switch(key.Value)
                        {
                            case "id":
                                if(doc.GetElementByID(this["id"]) != null) { doc.tagsByID.TryRemove(this["id"], out _); }
                                doc.tagsByID[value] = this;
                                break;
                            case "class":
                                string[] classes = value.Split(WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                string[] oldClasses = this["class"].Split(WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                foreach (string className in oldClasses)
                                {
                                    if(doc.tagsByClass.TryGetValue(className, out List<Tag> tags)) { tags.Remove(this); }
                                }
                                foreach (string className in classes)
                                {
                                    doc.tagsByClass.TryGetValue(className, out List<Tag> tags);
                                    if (tags == null)
                                    {
                                        tags = new List<Tag>(5);
                                        doc.tagsByClass[className] = tags;
                                    }
                                    tags.Add(this);
                                }
                                break;
                        }
                        tagAttributes[(this, AttributeName)] = value;
                    }
                }
            }

            public Tag Copy() => new Tag(this);            

            public void AddAttributes(string attributes)
            {
                if(this.IsImmutable) { return; }
                if(attributes.Contains("\0")) { attributes = String.Join(null, attributes.Split('\0')); }
                string[] tokens = GetTokens();
                int position = 0;
                while(position < tokens.Length)
                {
                    if(tokens[position + 1] == "=" && position < (tokens.Length + 2))
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
                    string[] results = rawChars.ToString().Split(WhiteSpaceChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for(int i = 0; i < results.Length; i++)
                    {
                        if(results[i][0] == '\0') { results[i] = strLiterals.Dequeue(); }
                    }
                    return results;
                }
            }

            public bool AddChild(Tag child)
            {
                if(this.IsImmutable || this.Contains(child)) { return false; }
                child.Parent?.RemoveChild(child);
                child.Parent = this;
                this.children.Add(child);
                HTML doc = this.Document as HTML;
                doc.tagsByTagName.TryGetValue(child.Name, out List<Tag> tags);
                if(tags == null)
                {
                    tags = new List<Tag>(10);
                    doc.tagsByTagName[child.Name] = tags;
                }
                tags.Add(child);                
                return true;
            }

            public void AddChildren(params Tag[] children) => Array.ForEach(children, (Tag child) => this.AddChild(child));
            public bool RemoveChild(Tag child)
            {
                if (this.IsImmutable) { return false; }
                return this.children.Remove(child);
            }

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
    }

    public sealed class ImmutableHTML : HTML
    {
        private ImmutableHTML(FileIO.FileData file) => this.RootElement = HTML.FromHTMLFile(Encoding.UTF8.GetString(file.Data)).RootElement;
        private ImmutableHTML(string content) => this.RootElement = HTML.FromHTMLFile(content).RootElement;
        public static new ImmutableHTML FromHTMLFile(string file) => new ImmutableHTML(FileIO.GetFile(file));
        public static new ImmutableHTML FromString(string str) => new ImmutableHTML(str);
    }
}