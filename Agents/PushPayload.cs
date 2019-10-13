// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace PhoneVoIPApp.Agents {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17613")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="WPNotification")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="WPNotification", IsNullable=false)]
    public partial class Notification {
        
        private string nameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        private string numberField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Number {
            get {
                return this.numberField;
            }
            set {
                this.numberField = value;
            }
        }

        private string locKey;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LocKey
        {
            get
            {
                return this.locKey;
            }
            set
            {
                this.locKey = value;
            }
        }

        private string locArguments;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LocArguments
        {
            get
            {
                return this.locArguments;
            }
            set
            {
                this.locArguments = value;
            }
        }

        private string data;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }
    }
}
