﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace vobla.Properties {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("vobla.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Login.
        /// </summary>
        public static string AccountLoginButton {
            get {
                return ResourceManager.GetString("AccountLoginButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Email address.
        /// </summary>
        public static string AccountLoginEmailLabel {
            get {
                return ResourceManager.GetString("AccountLoginEmailLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Password.
        /// </summary>
        public static string AccountLoginPWLabel {
            get {
                return ResourceManager.GetString("AccountLoginPWLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на The link was put into your clipboard..
        /// </summary>
        public static string BalloontipUploadedText {
            get {
                return ResourceManager.GetString("BalloontipUploadedText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на File uploaded.
        /// </summary>
        public static string BalloontipUploadedTitle {
            get {
                return ResourceManager.GetString("BalloontipUploadedTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на File has not been uploaded to the server..
        /// </summary>
        public static string BalloontipUploadErrorText {
            get {
                return ResourceManager.GetString("BalloontipUploadErrorText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Upload error.
        /// </summary>
        public static string BalloontipUploadErrorTitle {
            get {
                return ResourceManager.GetString("BalloontipUploadErrorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Поиск локализованного ресурса типа System.Drawing.Icon, аналогичного (Значок).
        /// </summary>
        public static System.Drawing.Icon favicon {
            get {
                object obj = ResourceManager.GetObject("favicon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Exit.
        /// </summary>
        public static string NotifyExit {
            get {
                return ResourceManager.GetString("NotifyExit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на genkstacloud.
        /// </summary>
        public static string NotifyText {
            get {
                return ResourceManager.GetString("NotifyText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на oblaco settings.
        /// </summary>
        public static string SettingsWindowTitle {
            get {
                return ResourceManager.GetString("SettingsWindowTitle", resourceCulture);
            }
        }
    }
}