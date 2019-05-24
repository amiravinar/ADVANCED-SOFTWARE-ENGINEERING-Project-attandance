package com.recogntion.ui;

import lombok.extern.slf4j.Slf4j;
import com.recogntion.faceengine.FaceRecognition;
import static com.recogntion.ui.Home.home;

import javax.swing.*;
import javax.swing.plaf.FontUIResource;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.io.File;
import java.io.IOException;
import java.util.Objects;

/**
 *
 */

public class FaceRecogntionUI {

    public static final String FACE_RECOGNITION_SRC_MAIN_RESOURCES = "src/main/resources/images";
    private static final String BASE_PATH = "src/main/resources/";
    private JFrame mainFrame;
    private static  JPanel mainPanel;
    private static final int FRAME_WIDTH = 1024;
    private static final int FRAME_HEIGHT = 420;
    private static ImagePanel sourceImagePanel;
    private static  FaceRecognition faceRecognition;
    private static  File selectedFile;
    private static JTextField memberNameField;
    private final Font sansSerifBold = new Font("SansSerif", Font.BOLD, 28);
    private static JPanel membersPhotosPanel;
    private static JScrollPane scrollMembersPhotos;
    
    public FaceRecogntionUI() throws Exception {
        UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
        UIManager.put("Button.font", new FontUIResource(new Font("Dialog", Font.BOLD, 18)));
        UIManager.put("ProgressBar.font", new FontUIResource(new Font("Dialog", Font.BOLD, 18)));
        faceRecognition = Home.faceRecognition;
    }
    
    public void initUI() throws Exception {

        // create main frame
        mainFrame = createMainFrame();
        
        mainPanel = new JPanel(new BorderLayout());
        addSignature();
        
        JButton chooseButton = new JButton("Choose Member Image");
        chooseButton.addActionListener(e -> {
            chooseFileAction();
            mainPanel.updateUI();
        });
        
        JButton registerNewMemberButton = new JButton("Register New Member");
        registerNewMemberButton.addActionListener(new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent event) {
                try {
                     registerUser(selectedFile.getAbsolutePath());
                } catch (Exception e) {
                    System.out.println(e);
                    throw new RuntimeException(e);
                }
            }
        });
        
        fillMainPanel(chooseButton, registerNewMemberButton);
        
        mainPanel.updateUI();
        
        mainFrame.add(mainPanel, BorderLayout.CENTER);
        
        mainFrame.setVisible(true);
        
    }
    
     public static void registerUser(String img){
           try {
                  selectedFile=new File(img);
                    addPhoto(selectedFile.getAbsolutePath(), memberNameField.getText());
                    membersPhotosPanel.updateUI();
                    scrollMembersPhotos.updateUI();
                    mainPanel.updateUI();
                } catch (IOException e) {
                    System.out.println(e);
                    throw new RuntimeException(e);
                }
    }
    
    private void fillMainPanel(JButton chooseButton, Component registerNewMemberButton) throws IOException {
        
        GridLayout layout = new GridLayout(1, 3);
        layout.setHgap(1);
        JPanel panelRegister = new JPanel(layout);
        memberNameField = new JTextField();
        JLabel lblmemberName = new JLabel("Member Name");
        lblmemberName.setFont(new Font(Font.DIALOG, Font.BOLD, 18));
        panelRegister.add(chooseButton);
        panelRegister.add(lblmemberName);
        panelRegister.add(memberNameField);
        panelRegister.add(registerNewMemberButton);
        
        mainPanel.add(panelRegister, BorderLayout.NORTH);
        
        membersPhotosPanel = new JPanel(new GridLayout(1, 15));
        scrollMembersPhotos = new JScrollPane(membersPhotosPanel);
        mainPanel.add(scrollMembersPhotos, BorderLayout.SOUTH);
        
        File[] files = new File(BASE_PATH + "/images").listFiles();
        for (File file : Objects.requireNonNull(files)) {
            File[] images = file.listFiles();
            addPhoto(Objects.requireNonNull(images)[0].getAbsolutePath());
            
        }
        sourceImagePanel = new ImagePanel(150, 150);
        JPanel jPanel = new JPanel(new GridLayout(1, 2));
        jPanel.add(sourceImagePanel);
        
        mainPanel.add(jPanel, BorderLayout.CENTER);
        mainPanel.updateUI();
        
    }
    
    private void addPhoto(String path) throws IOException {
        addPhoto(path, null);
    }
    
    private static  void addPhoto(String path, String name) throws IOException {
        ImagePanel imagePanel = new ImagePanel(150, 150, new File(path), name);
        faceRecognition.registerNewMember(imagePanel.getTitle(), new File(imagePanel.getFilePath()).getAbsolutePath());
        membersPhotosPanel.add(imagePanel);
    }
    
    public void chooseFileAction() {
        JFileChooser chooser = new JFileChooser();
        chooser.setCurrentDirectory(new File(new File(FACE_RECOGNITION_SRC_MAIN_RESOURCES).getAbsolutePath()));
        int action = chooser.showOpenDialog(null);
        if (action == JFileChooser.APPROVE_OPTION) {
            try {
                selectedFile = chooser.getSelectedFile();
                sourceImagePanel.setImage(selectedFile.getAbsolutePath());
            } catch (Exception e) {
            	 System.out.println(e);
                throw new RuntimeException(e);
            }
        }
    }
    
    private JFrame createMainFrame() {
        JFrame mainFrame = new JFrame();
        mainFrame.setTitle("Face Recognizer");
        mainFrame.setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE);
        mainFrame.setSize(FRAME_WIDTH, FRAME_HEIGHT);
        mainFrame.setLocationRelativeTo(null);
        mainFrame.addWindowListener(new WindowAdapter() {
            @Override
            public void windowClosed(WindowEvent e) {
                System.exit(0);
            }
        });
        return mainFrame;
    }
    
    private void addSignature() {
        JLabel signature = new JLabel("Face Recognisition Attendance System", SwingConstants.CENTER);
        signature.setFont(new Font(Font.SANS_SERIF, Font.ITALIC, 20));
        signature.setForeground(Color.BLUE);
        JButton backBtn = new JButton("<< Back");
        backBtn.setForeground(Color.BLUE);
        backBtn.addActionListener((e) -> {
            mainFrame.setVisible(false);
            home.setVisible(true);
        });
        mainFrame.add(backBtn,BorderLayout.BEFORE_FIRST_LINE);
        mainFrame.add(signature, BorderLayout.NORTH);
        
         JButton captBtn = new JButton("Capture Image");
        captBtn.setForeground(Color.BLUE);
        captBtn.addActionListener((e) -> {
            SwingUtilities.invokeLater(new WebcamViewerExample("register"));
            
        });
        mainFrame.add(captBtn,BorderLayout.BEFORE_FIRST_LINE);
        
    }
    
}
