import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.File;
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
import java.util.logging.Level;
import java.util.logging.Logger;

public class asset_server_2 implements Runnable {
	private final static Logger logger = Logger.getLogger(asset_server_2.class.getCanonicalName());
	private File rootDirectory;
	private String indexFileName = "index.html";
	private Socket connection;
	private HashMap<String, String> asset_db = new HashMap<String, String>();

	public asset_server_2(File rootDirectory, String indexFileName, Socket connection) {
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
		set_asset_db(asset_db);

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

				if (fileName.startsWith("/asset/file")) {
					String[] query = fileName.substring(fileName.indexOf("?") + 1).split("&");
					String userId = null;
					String assetId = null;

					for (String param : query) {
						String[] pair = param.split("=");
						String key = URLDecoder.decode(pair[0], StandardCharsets.UTF_8);
						String value = URLDecoder.decode(pair[1], StandardCharsets.UTF_8);

						if (key.equals("user_id")) {
							userId = value;
						} else if (key.equals("ast_id")) {
							assetId = value;
						}
					}

					// 각 키 값이 null이 아니고 assetId가 해시맵에 존재하는 경우
					if (userId != null && assetId != null && asset_db.containsKey(assetId)) {
						String assetFileName = asset_db.get(assetId);
						theFile = new File(rootDirectory, assetFileName);

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
			} else {
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

	private void set_asset_db(HashMap<String, String> db) {
		db.put("1", "duck.glb");
		db.put("2", "camera_1.glb");
		db.put("3", "camera_gib.glb");
		db.put("4", "chain_box.glb");
		db.put("5", "chair.glb");
		db.put("6", "table.glb");
		db.put("7", "lamp_1.glb");
		db.put("8", "lamp_2.glb");
		db.put("9", "notebook.glb");
	}
}

//ex)  Get /localhost/asset?id=1
