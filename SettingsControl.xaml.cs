namespace Flow.Launcher.Plugin.WolframAlpha
{
    public partial class SettingsControl 
    {
        public Settings _settings { get; }

        public SettingsControl(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
        }
    }
}