# Kinect .NET Framework Project

This project demonstrates how to use the Kinect sensor with a WPF application to track and interact with skeleton data. The application detects hand movements and displays corresponding messages on the UI.

## Features

- Track skeleton data using Kinect sensor
- Detect hand shakes and display a "Hello!" message
- Update UI elements based on hand movements
- Count and display the number of fingers raised (random number, to be continued)

## Requirements

- Kinect sensor
- .NET Framework 4.7.2
- Visual Studio 2019 or later
- Kinect SDK v1.8

## Getting Started

1. **Clone the repository:**
    ```
    git clone https://github.com/Rebecabrk/Kinect_dotnet_framework_application
    ```

2. **Open the project in Visual Studio:**
    - Open `Kinect_dotnet_framework.sln` in Visual Studio.

3. **Build the project:**
    - Build the solution to restore the necessary NuGet packages and compile the project.

4. **Run the application:**
    - Connect your Kinect sensor.
    - Run the application from Visual Studio.

## Project Structure

- **MainWindow.xaml.cs**: Contains the main logic for handling Kinect sensor data and updating the UI.
- **MainWindow.xaml**: Defines the UI layout for the application.
- **App.xaml**: Application configuration file.

## Usage

- **Hand Shake Detection**: Move your right hand left and right to trigger a "Hello!" message.
- **Finger Counting**: Raise your hand above your chest to count and display the number of fingers raised (random number, to be continued).

## Troubleshooting

- Ensure the Kinect sensor is properly connected and recognized by your system.
- Verify that the Kinect SDK is installed and up to date.
- Check for any exceptions or errors in the Visual Studio output window.

## Acknowledgements

- [Emgu CV](http://www.emgu.com/wiki/index.php/Main_Page) for image processing.
- [Microsoft Kinect SDK](https://developer.microsoft.com/en-us/windows/kinect/) for Kinect sensor integration.
