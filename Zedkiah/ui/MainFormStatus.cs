using System.Windows;
using Stateless;

namespace Zedkiah.ui;

public class MainFormState
{
    public MainFormStatus Status { get; set; }
    
    
    public enum MainFormStatus
    {
        Initializing,
        Ready,
        Connecting,
        Connected,
        Disconnecting,
        Error
    }
    
    public enum MainTabStatus
    {
        
    }
}