# ESP Flasher Tool

The ESP Flasher Tool simplifies the process of flashing ESP32 devices during production. It provides a user-friendly GUI, eliminating the need for command-line tools. Production employees can manage firmware programming and flash memory operations without using esptool or Python, as everything is natively built in .NET.

## Interface Overview

![image](https://github.com/user-attachments/assets/1ace11d7-0d76-4925-a808-0596ef45e2ca)


### 1. Archive Section
- **FileName**: Lists the binary files to be flashed to the ESP32 device.
- **Address**: Specifies the memory address for flashing each file.
- **Size**: Displays the size of the binary in hexadecimal format.
- **Partition Table**: Provides detailed partition information such as:
  - **Name**: Name of the partition.
  - **Type**: Type of partition (e.g., data, application).
  - **Subtype**: Further defines the partition type.
  - **Address**: Flash memory address.
  - **Size**: Size of the partition in hexadecimal format.
  
### 2. Serial Port Section
- **Baudrate**: Selects the communication speed (e.g., 921600) for connecting to the ESP32 device.
- **Serial Port**: Allows the user to choose the correct COM port to which the ESP32 device is connected.
- **Refresh**: Refreshes the list of available COM ports.

### 3. Actions Section
- **Program**: Uploads the firmware files listed in the archive to the ESP32 device.
- **Erase**: Erases all data from the device's flash memory.
- **Use Compression**: Optionally compresses the firmware files before uploading them to the device.

### 4. Progress Section
- **Progress Bar**: Visually indicates the current status of the flashing or erasing operation.
- **Cancel**: Stops the ongoing operation.

### 5. Log Section
- **Logs**: Displays detailed log messages, providing feedback about the tool's operations.

## Additional Features

### Handling KCZIP Files
- KCZIP is a zip file format containing multiple binaries and a JSON file that specifies where these binaries should be placed in flash memory.
- You can open a `.kczip` file using **File -> Open Archive** and save it again using **File -> Save as -> Archive**.

### Saving as Intel HEX Files
- The ESP Flasher Tool supports saving as Intel HEX files. There are two options:
  - **Application Intel HEX**: Saves only the application binary (OTA partition) as a HEX file.
  - **Archive Intel HEX**: Combines all binary files into a single Intel HEX file.

### Create Archive from Build Results
- You can generate an archive directly from your build results by selecting **Development -> Open Build Folder**. Choose the `flash_project_args` file, and the tool will automatically resolve and include the necessary binaries for flashing.

