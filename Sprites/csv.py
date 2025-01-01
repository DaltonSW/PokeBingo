import os

with open("./out.csv", "w") as outFile:
    outFile.write("file,name,gens,methods,nat_dex\n")
    for filename in os.listdir("."):
        if filename.endswith(".png"):
            outFile.write(filename + ",,,,\n")
