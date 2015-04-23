using Awesomium.Core;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS14.Client.UI
{
    public static class SfmlAwesomiumInterop
    {
        /// <summary>
        /// Converts KeyEventArgs into a WebKeyboardEvent for injection into a WebView.
        /// </summary>
        /// <param name="kea">The original event args.</param>
        /// <param name="type">Event type.  'KeyDown' and 'KeyUp' only.</param>
        public static WebKeyboardEvent ToWebKeyboardEvent(this KeyEventArgs kea, WebKeyboardEventType type)
        {
            var wke = new WebKeyboardEvent();
            if (kea.Alt) wke.Modifiers |= Modifiers.AltKey;
            if (kea.Control) wke.Modifiers |= Modifiers.ControlKey;
            if (kea.Shift) wke.Modifiers |= Modifiers.ShiftKey;
            if (kea.System) wke.Modifiers |= Modifiers.MetaKey;
            wke.IsSystemKey = kea.System;
            wke.VirtualKeyCode = kea.Code.ToVirtualKey();
            wke.Type = type;
            return wke;

        }

        /// <summary>
        /// Converts an SFML key to the corresponding VirtualKey.
        /// </summary>
        /// <param name="key">A Keyboard.Key provided by SFML.</param>
        public static VirtualKey ToVirtualKey(this Keyboard.Key key)
        {
            switch (key)
            {
                case Keyboard.Key.LShift: return VirtualKey.LSHIFT;
                case Keyboard.Key.RShift: return VirtualKey.RSHIFT;
                case Keyboard.Key.LAlt: return VirtualKey.LMENU;
                case Keyboard.Key.RAlt: return VirtualKey.RMENU;
                case Keyboard.Key.LControl: return VirtualKey.LCONTROL;
                case Keyboard.Key.RControl: return VirtualKey.RCONTROL;
                case Keyboard.Key.LSystem: return VirtualKey.LWIN;
                case Keyboard.Key.RSystem: return VirtualKey.RWIN;

                case Keyboard.Key.Menu: return VirtualKey.APPS;
                case Keyboard.Key.SemiColon: return VirtualKey.OEM_1;   
                case Keyboard.Key.Slash: return VirtualKey.OEM_2;      
                case Keyboard.Key.Equal: return VirtualKey.OEM_PLUS;   
                case Keyboard.Key.Dash: return VirtualKey.OEM_MINUS;  
                case Keyboard.Key.LBracket: return VirtualKey.OEM_4;      
                case Keyboard.Key.RBracket: return VirtualKey.OEM_6;      
                case Keyboard.Key.Comma: return VirtualKey.OEM_COMMA;  
                case Keyboard.Key.Period: return VirtualKey.OEM_PERIOD; 
                case Keyboard.Key.Quote: return VirtualKey.OEM_7;      
                case Keyboard.Key.BackSlash: return VirtualKey.OEM_5;      
                case Keyboard.Key.Tilde: return VirtualKey.OEM_3;      
                case Keyboard.Key.Escape: return VirtualKey.ESCAPE;     
                case Keyboard.Key.Space: return VirtualKey.SPACE;      
                case Keyboard.Key.Return: return VirtualKey.RETURN;     
                case Keyboard.Key.BackSpace: return VirtualKey.BACK;       
                case Keyboard.Key.Tab: return VirtualKey.TAB;        
                case Keyboard.Key.PageUp: return VirtualKey.PRIOR;      
                case Keyboard.Key.PageDown: return VirtualKey.NEXT;       
                case Keyboard.Key.End: return VirtualKey.END;        
                case Keyboard.Key.Home: return VirtualKey.HOME;       
                case Keyboard.Key.Insert: return VirtualKey.INSERT;     
                case Keyboard.Key.Delete: return VirtualKey.DELETE;     
                case Keyboard.Key.Add: return VirtualKey.ADD;        
                case Keyboard.Key.Subtract: return VirtualKey.SUBTRACT;   
                case Keyboard.Key.Multiply: return VirtualKey.MULTIPLY;   
                case Keyboard.Key.Divide: return VirtualKey.DIVIDE;     
                case Keyboard.Key.Pause: return VirtualKey.PAUSE;      
                case Keyboard.Key.F1: return VirtualKey.F1;         
                case Keyboard.Key.F2: return VirtualKey.F2;         
                case Keyboard.Key.F3: return VirtualKey.F3;         
                case Keyboard.Key.F4: return VirtualKey.F4;         
                case Keyboard.Key.F5: return VirtualKey.F5;         
                case Keyboard.Key.F6: return VirtualKey.F6;         
                case Keyboard.Key.F7: return VirtualKey.F7;         
                case Keyboard.Key.F8: return VirtualKey.F8;         
                case Keyboard.Key.F9: return VirtualKey.F9;         
                case Keyboard.Key.F10: return VirtualKey.F10;        
                case Keyboard.Key.F11: return VirtualKey.F11;        
                case Keyboard.Key.F12: return VirtualKey.F12;        
                case Keyboard.Key.F13: return VirtualKey.F13;        
                case Keyboard.Key.F14: return VirtualKey.F14;        
                case Keyboard.Key.F15: return VirtualKey.F15;        
                case Keyboard.Key.Left: return VirtualKey.LEFT;       
                case Keyboard.Key.Right: return VirtualKey.RIGHT;      
                case Keyboard.Key.Up: return VirtualKey.UP;         
                case Keyboard.Key.Down: return VirtualKey.DOWN;       
                case Keyboard.Key.Numpad0: return VirtualKey.NUMPAD0;    
                case Keyboard.Key.Numpad1: return VirtualKey.NUMPAD1;    
                case Keyboard.Key.Numpad2: return VirtualKey.NUMPAD2;    
                case Keyboard.Key.Numpad3: return VirtualKey.NUMPAD3;    
                case Keyboard.Key.Numpad4: return VirtualKey.NUMPAD4;    
                case Keyboard.Key.Numpad5: return VirtualKey.NUMPAD5;    
                case Keyboard.Key.Numpad6: return VirtualKey.NUMPAD6;    
                case Keyboard.Key.Numpad7: return VirtualKey.NUMPAD7;    
                case Keyboard.Key.Numpad8: return VirtualKey.NUMPAD8;    
                case Keyboard.Key.Numpad9: return VirtualKey.NUMPAD9;    
                case Keyboard.Key.A: return VirtualKey.A;           
                case Keyboard.Key.Z: return VirtualKey.Z;           
                case Keyboard.Key.E: return VirtualKey.E;           
                case Keyboard.Key.R: return VirtualKey.R;           
                case Keyboard.Key.T: return VirtualKey.T;           
                case Keyboard.Key.Y: return VirtualKey.Y;           
                case Keyboard.Key.U: return VirtualKey.U;           
                case Keyboard.Key.I: return VirtualKey.I;           
                case Keyboard.Key.O: return VirtualKey.O;           
                case Keyboard.Key.P: return VirtualKey.P;           
                case Keyboard.Key.Q: return VirtualKey.Q;           
                case Keyboard.Key.S: return VirtualKey.S;           
                case Keyboard.Key.D: return VirtualKey.D;           
                case Keyboard.Key.F: return VirtualKey.F;           
                case Keyboard.Key.G: return VirtualKey.G;           
                case Keyboard.Key.H: return VirtualKey.H;           
                case Keyboard.Key.J: return VirtualKey.J;           
                case Keyboard.Key.K: return VirtualKey.K;           
                case Keyboard.Key.L: return VirtualKey.L;           
                case Keyboard.Key.M: return VirtualKey.M;           
                case Keyboard.Key.W: return VirtualKey.W;           
                case Keyboard.Key.X: return VirtualKey.X;           
                case Keyboard.Key.C: return VirtualKey.C;           
                case Keyboard.Key.V: return VirtualKey.V;           
                case Keyboard.Key.B: return VirtualKey.B;
                case Keyboard.Key.N: return VirtualKey.N;           
                case Keyboard.Key.Num0: return VirtualKey.NUM_0;           
                case Keyboard.Key.Num1: return VirtualKey.NUM_1;           
                case Keyboard.Key.Num2: return VirtualKey.NUM_2;           
                case Keyboard.Key.Num3: return VirtualKey.NUM_3;           
                case Keyboard.Key.Num4: return VirtualKey.NUM_4;           
                case Keyboard.Key.Num5: return VirtualKey.NUM_5;           
                case Keyboard.Key.Num6: return VirtualKey.NUM_6;           
                case Keyboard.Key.Num7: return VirtualKey.NUM_7;           
                case Keyboard.Key.Num8: return VirtualKey.NUM_8;           
                case Keyboard.Key.Num9: return VirtualKey.NUM_9;
                default: throw new NotImplementedException(); // Fill these in as we come across them.
            }
        }
    }
}
