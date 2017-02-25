# CachedResourceManager
Application with many instances of custom controls will spend a lot time initializing them from resx files. Every call InitializeComponent will load data from resources. This data can be cached.
Replace standart ComponentResourceManager with CachedComponentResourceManager and you'll save time on control initialization.
You need to make changes by hands in control_name.Designer.cs file.

# Example

    partial class ShellForm
    {
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            CachedComponentResourceManager resources = new CachedComponentResourceManager(typeof(ShellForm));
        }
    }
