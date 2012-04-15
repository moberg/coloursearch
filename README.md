Colour Search
=============
ColourSearch is a image search engine I wrote to expieriment 
with histogram based image comparison. 

How to run
----------
ColourSearch is self hosted (it contains a small web server) and 
no configuration is needed to run it. By default it will build a 
database based on the images located in .\public\Content\Images.

To run the application, build it and then run: coloursearch.exe

Then browse to http://localhost:9200


### Options
 .\coloursearch.exe --help
Usage: coloursearch [OPTIONS]+ message

Options:
  -p, --port=VALUE           port to run the service on.
  -d, --database=VALUE       database file.
  -m, --multithreaded        run multithreaded server.
  -i, --images=VALUE         images directory.
  -s, --staticfiles=VALUE    static files directory.
  -r, --rebuild              rebuild the database.
  -h, --help                 show this message and exit
