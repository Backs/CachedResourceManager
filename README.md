# CachedResourceManager
Application with many instances of custom controls will spend a lot time initializing them from resx files. Every call InitializeComponent will load data from resources. This data can be cached.
Replace standart ComponentResourceManager with CachedComponentResourceManager and you'll save time on control initialization.
