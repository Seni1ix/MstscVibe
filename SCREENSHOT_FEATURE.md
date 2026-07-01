# Screenshot Feature Implementation

## Summary
Added screenshot functionality to MstscVibe with the following components:

## Changes Made

### 1. **UserSettings.cs**
- Added `ScreenshotPath` property with default value
- Implemented `GetDefaultScreenshotPath()` method that defaults to `%UserProfile%\Pictures\mstsc`
- Screenshot path is automatically persisted in settings.json

### 2. **OptionsForm.cs**
- Added new "Screenshots" group box with:
  - Text input for screenshot save location
  - "Browse..." button to select folder via FolderBrowserDialog
  - Loads current screenshot path from UserSettings on form load
  - Saves selected path when OK is clicked
- Increased form height from 310 to 460 pixels to accommodate new section
- Added `ScreenshotPath` property to expose selected path

### 3. **SessionForm.cs**
- Added new system menu constant: `SC_TAKE_SCREENSHOT = 0xF131`
- Added "Take Screenshot" menu item to system menu
- Implemented screenshot handling in `WndProc()` method
- Created `TakeScreenshot()` method that:
  - Validates RDP connection is active
  - Loads screenshot path from UserSettings
  - Creates directory if it doesn't exist
  - Captures current RDP session view via `DrawToBitmap()`
  - Saves as PNG with timestamp: `mstsc_YYYY-MM-DD_HH-mm-ss-fff.png`
  - Shows success/error message to user

## Features
✅ **System Menu Integration** - Screenshot option available in window system menu
✅ **Default Location** - Automatically defaults to `My Pictures\mstsc`
✅ **User-Configurable** - Can be changed in Options dialog
✅ **Automatic Directory Creation** - Creates screenshot folder if missing
✅ **Timestamped Filenames** - Each screenshot uniquely named with date and time
✅ **User Feedback** - Shows success/error messages with file path
✅ **Session Validation** - Only allows screenshots when RDP session is connected
✅ **PNG Format** - Screenshots saved as PNG for lossless quality

## Usage
1. **Via System Menu**: Right-click window title bar → Take Screenshot
2. **Configure Location**: Main menu → Options → Screenshots → Browse...
3. **Default Location**: `C:\Users\{username}\Pictures\mstsc`

## Files Modified
- `UserSettings.cs` - Added screenshot path setting
- `OptionsForm.cs` - Added screenshot configuration UI
- `SessionForm.cs` - Added screenshot capture functionality
