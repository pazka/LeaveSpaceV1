String FILE_PATH = "C:\\Users\\Alexa\\Documents\\Projects\\Alessia\\LeaveSpace\\19500325--20230330.lte";

float min = 9999999.0;
float max = 0.0;

String[] parseFile(String path) {
  BufferedReader reader = createReader(path);
  String lines = {};
  try {
    while ((line = reader.readLine()) != null) {
      lines += line;
    }
    reader.close();
  } catch (IOException e) {
    e.printStackTrace();
  }
  
    return lines;
  
} 

String[] lines = parseFile(FILE_PATH);
println(lines[0])); //<>//
