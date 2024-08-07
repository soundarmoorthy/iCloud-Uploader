# iCloud-Uploader
Upload a bunch of files to ICloud drive in Mac. It handles duplicate files not by name but by contents, based on the [MD5 signature](https://en.wikipedia.org/wiki/MD5).


# Why did i wrote this
https://soundar.substack.com/p/where-i-dont-use-ai

# How to use this 
* In the Program.cs file replace the iCloud folder with the path you have mounted in Mac.
* In the Program.cs file replace the source folder with the path which has contents of your source path.

# Bugs
There is a race condition where parallelism of hashing and processing leads to situations where the duplicates are not detected in 1 pass. Running the tool repeatedly for 2-3 times sort of does that. 
