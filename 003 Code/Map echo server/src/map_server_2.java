import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.Reader;
import java.io.Writer;
import java.net.Socket;
import java.net.URLConnection;
import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.logging.Level;
import java.util.logging.Logger;

public class map_server_2 implements Runnable {
	private final static Logger logger = Logger.getLogger(map_server_2.class.getCanonicalName());
	private File rootDirectory;
	private String indexFileName = "index.html";
	private Socket connection;

	public map_server_2(File rootDirectory, String indexFileName, Socket connection) {
		if (rootDirectory.isFile()) {
			throw new IllegalArgumentException("rootDirectory must be a directory, not a file");
		}
		try {
			rootDirectory = rootDirectory.getCanonicalFile();
		} catch (IOException ex) {
		}
		this.rootDirectory = rootDirectory;
		if (indexFileName != null)
			this.indexFileName = indexFileName;
		this.connection = connection;
	}

	@Override
	public void run() {
		// for security checks

		String root = rootDirectory.getPath();
		try {
			OutputStream raw = new BufferedOutputStream(connection.getOutputStream());
			Writer out = new OutputStreamWriter(raw);
			Reader in = new InputStreamReader(new BufferedInputStream(connection.getInputStream()), "US-ASCII");
			StringBuilder requestLine = new StringBuilder();
			while (true) {
				int c = in.read();
				if (c == '\r' || c == '\n')
					break;
				requestLine.append((char) c);
			}
			String get = requestLine.toString();
			logger.info(connection.getRemoteSocketAddress() + " " + get);
			String[] tokens = get.split("\\s+");
			String method = tokens[0];
			String version = "";
			if (method.equals("GET")) {
				String fileName = tokens[1];
				String contentType = URLConnection.getFileNameMap().getContentTypeFor(fileName);
				File theFile;
				
				if(fileName.startsWith("/asset/map")) {
					String[] query = fileName.substring(fileName.indexOf("?") + 1).split("&");
					String mapId = null;
					String fileId = null;

					for (String param : query) {
						String[] pair = param.split("=");
						String key = URLDecoder.decode(pair[0], StandardCharsets.UTF_8);
						String value = URLDecoder.decode(pair[1], StandardCharsets.UTF_8);

						if (key.equals("map_id")) {
							mapId = value;
						} else if (key.equals("ast_id")) {
							fileId = value;
						}
					}

					// 각 키 값이 null이 아니면
					if (mapId != null && fileId != null) {
						String mapFileName = mapId + "_" + fileId + ".json";
						theFile = new File(rootDirectory, mapFileName);

						if (theFile.exists() && theFile.isFile() && theFile.canRead()) {
							// 파일이 존재하면서 읽을 수 있는 경우
							sendFile(out, raw, theFile, contentType);
						} else {
							// 파일이 존재하지 않는 경우
							String body = "<html><body><h1>File Not Found</h1></body></html>";
							send404(out, body);
						}
					} else {
						// 잘못된 요청 또는 파일이 존재하지 않는 경우
						String body = "<html><body><h1>Invalid Request or File Not Found</h1></body></html>";
						send404(out, body);
						return;
					}
				}
			}
			else if (method.equals("POST")) {
				StringBuilder requestBody = new StringBuilder();
			    while (in.ready()) {
			        requestBody.append((char) in.read());
			    }
			    
			    Map<String, String> formData = parseFormData(requestBody.toString());
			    
			    handlePostRequest(formData, out);
			}
			else {
				// GET 메서드 이외의 요청에 대한 응답
				String body = "<html><body><h1>Not Implemented</h1></body></html>";
				send501(out, body);
			}
		} catch (IOException ex) {
			logger.log(Level.WARNING, "Error talking to " + connection.getRemoteSocketAddress(), ex);
		} finally {
			try {
				connection.close();
			} catch (IOException ex) {
				ex.printStackTrace();
			}
		}
	}

	private void sendFile(Writer out, OutputStream raw, File theFile, String contentType) throws IOException {
		byte[] theData = Files.readAllBytes(theFile.toPath());

		// 응답 헤더 전송
		sendHeader(out, "HTTP/1.0 200 OK", contentType, theData.length);

		// 파일 데이터 전송
		raw.write(theData);
		raw.flush();
	}

	private void send404(Writer out, String body) throws IOException {
		sendHeader(out, "HTTP/1.0 404 File Not Found", "text/html", body.length());
		out.write(body);
		out.flush();
	}

	private void send501(Writer out, String body) throws IOException {
		sendHeader(out, "HTTP/1.0 501 Not Implemented", "text/html", body.length());
		out.write(body);
		out.flush();
	}

	private void sendHeader(Writer out, String responseCode, String contentType, int length) throws IOException {
		out.write(responseCode + "\r\n");
		Date now = new Date();
		out.write("Date: " + now + "\r\n");
		out.write("Server: JHTTP 2.0\r\n");
		out.write("Content-length: " + length + "\r\n");
		out.write("Content-type: " + contentType + "\r\n\r\n");
		out.flush();
	}
	
	private Map<String, String> parseFormData(String formData) {
		//logger.info(formData + "/n/n/n");
	    Map<String, String> formDataMap = new HashMap<>();
	    try {
	    	String[] HeaderAndBody = formData.split("\r\n\r\n");
	        String[] pairs = HeaderAndBody[1].split("&");
	        for (String pair : pairs) {
	        	System.out.print(pair + "\n");
	            String[] keyValue = pair.split("=");
	            String key = URLDecoder.decode(keyValue[0], StandardCharsets.UTF_8);
	            String value = URLDecoder.decode(keyValue[1], StandardCharsets.UTF_8);
	            formDataMap.put(key, value);
	        }
	    } catch (Exception e) {
	        e.printStackTrace();
	    }
	    return formDataMap;
	}
	
	// POST 요청을 처리하고 클라이언트에게 적절한 응답을 보냄
	private void handlePostRequest(Map<String, String> formData, Writer out) throws IOException {
	    // 폼 데이터에서 필요한 작업을 수행하고 응답을 생성
	    String mapId = formData.get("mapId");
	    String num = formData.get("Num");
	    String json = formData.get("json");

	    //logger.info("Received POST request - mapId: " + mapId + ", num: " + num);
	    
	    // 파일 저장 경로 설정
	    String fileName = mapId + "_" + num + ".json";
	    File filePath = new File(rootDirectory, fileName);

	    // 파일에 데이터 저장
	    try (BufferedWriter writer = new BufferedWriter(new FileWriter(filePath))) {
	        writer.write(json);
	    } catch (IOException e) {
	        // 파일 저장 중 오류 발생 시 예외 처리
	        logger.log(Level.SEVERE, "Error saving file: " + e.getMessage());
	        String response = "Save file error";
	        sendHeader(out, "500 Internal Server Error", "Error saving file", response.length());
	        out.write(response);
	        out.flush();
	        return;
	    }

	    // 응답 전송
	    String responseBody = "Data received and saved successfully"; // 예시로 간단한 응답을 생성
	    sendHeader(out, "HTTP/1.0 200 OK", "text/plain", responseBody.length());
	    out.write(responseBody);
	    out.flush();
	}

}

//ex)  Get /localhost/asset?id=1
