using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;

namespace ClipboardCleaner
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private bool _isAppEnabled = true;
        private NSStatusItem _item;
        private NSWindowController _controller;
        private NSTimer _clipboardTimer;
        private string _currentClipboardText = string.Empty;

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var storyboard = NSStoryboard.FromName("Main", null);
            _controller = storyboard.InstantiateControllerWithIdentifier("WindowController") as NSWindowController;

            _item = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
            _item.Button.Image = NSImage.ImageNamed("status_enabled");
            _item.Menu = new NSMenu();

            var watcherItem = new NSMenuItem("Disable watcher");
            watcherItem.Activated += (sender, e) => {
                _isAppEnabled = !_isAppEnabled;
                watcherItem.Title = _isAppEnabled ? $"Disable watcher" : $"Enable watcher";
                _item.Button.Image = _isAppEnabled ? NSImage.ImageNamed("status_enabled") : NSImage.ImageNamed("status_disabled");

                if (_isAppEnabled)
                {
                    StartTimer();
                }
                else
                {
                    StopTimer();
                }

            };


            var aboutItem = new NSMenuItem("About");
            aboutItem.Activated += (sender, e) => {
                if (_controller?.Window?.IsVisible == false)
                {
                    _controller.ShowWindow(this);
                }
            };

            var exitItem = new NSMenuItem("Exit");
            exitItem.Activated += (sender, e) => {

                NSApplication.SharedApplication.Terminate(this);

            };

            _item.Menu.AddItem(watcherItem);
            _item.Menu.AddItem(aboutItem);
            _item.Menu.AddItem(NSMenuItem.SeparatorItem);
            _item.Menu.AddItem(exitItem);

            StartTimer();
        }

        private void StopTimer()
        {
            _clipboardTimer.Invalidate();
            _clipboardTimer.Dispose();
            _clipboardTimer = null;
        }

        private void StartTimer()
        {
            _clipboardTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromMilliseconds(500), delegate {

                var text = NSPasteboard.GeneralPasteboard.GetStringForType(NSPasteboard.NSPasteboardTypeString) ?? string.Empty;

                if (!_currentClipboardText.Equals(text) && DoesClipboardContainsText(NSPasteboard.GeneralPasteboard.Types))
                {
                    NSPasteboard.GeneralPasteboard.ClearContents();
                    NSPasteboard.GeneralPasteboard.DeclareTypes(new string[] { NSPasteboard.NSPasteboardTypeString }, null);
                    NSPasteboard.GeneralPasteboard.SetStringForType(text, NSPasteboard.NSPasteboardTypeString);
                    _currentClipboardText = text;
                    Console.WriteLine($"Overwritten: {text}");
                }

                Console.WriteLine($"Clipboard: {text}");
            });

            _clipboardTimer.Fire();
        }

        private bool DoesClipboardContainsText(string[] types)
        {
            if (!NSPasteboard.GeneralPasteboard.Types.Contains(NSPasteboard.NSPasteboardTypeString))
            {
                return false;
            }

            var blacklistedTypes = new List<string>
            {
                NSPasteboard.NSPasteboardTypePNG,
                NSPasteboard.NSPasteboardTypeTIFF,
                NSPasteboard.NSPasteboardTypeFileUrl
            };

            var whitelistedTypes = new List<string>
            {
                NSPasteboard.NSPasteboardTypeRTF,
                NSPasteboard.NSPasteboardTypeRTFD,
                NSPasteboard.NSPasteboardTypeHTML
            };

            var containsBlackListedValue = types.Any(item => blacklistedTypes.Contains(item));
            var containsWhiteListedValue = types.Any(item => whitelistedTypes.Contains(item));

            if(containsWhiteListedValue && !containsBlackListedValue)
            {
                return true;
            }

            return false;
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
