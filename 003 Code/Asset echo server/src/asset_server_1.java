import java.io.File;
import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.logging.Level;
import java.util.logging.Logger;

public class asset_server_1 {
	private static final Logger logger = Logger.getLogger(asset_server_1.class.getCanonicalName());
	private static final int NUM_THREADS = 50;
	private static final String INDEX_FILE = "index.html";
	private final File rootDirectory;
	private final int port;

	public asset_server_1(File rootDirectory, int port) throws IOException {
	 if (!rootDirectory.isDirectory()) {
	 throw new IOException(rootDirectory
	 + " does not exist as a directory");
	 }
	 this.rootDirectory = rootDirectory;
	 this.port = port;
	 }

	public void start() throws IOException {
		ExecutorService pool = Executors.newFixedThreadPool(NUM_THREADS);
		try (ServerSocket server = new ServerSocket(port)) {
			logger.info("Accepting connections on port " + server.getLocalPort());
			logger.info("Document Root: " + rootDirectory);
			while (true) {
				try {
					Socket request = server.accept();
					Runnable r = new asset_server_2(rootDirectory, INDEX_FILE, request);
					pool.submit(r);
				} catch (IOException ex) {
					logger.log(Level.WARNING, "Error accepting connection", ex);
				}
			}
		}
	}

	public static void main(String[] args) {
		// get the Document root
		File docroot = new File("server_root");
//		try {
//			docroot = new File("server_root");
//		} catch (ArrayIndexOutOfBoundsException ex) {
//			System.out.println("Usage: java JHTTP docroot port");
//			return;
//		}
		// set the port to listen on
		int port = 2002;
//		try {
//			port = Integer.parseInt(args[1]);
//			if (port < 0 || port > 65535)
//				port = 80;
//		} catch (RuntimeException ex) {
//			port = 80;
//		}
		try {
			asset_server_1 webserver = new asset_server_1(docroot, port);
			webserver.start();
		} catch (IOException ex) {
			logger.log(Level.SEVERE, "Server could not start", ex);
		}
	}
}
