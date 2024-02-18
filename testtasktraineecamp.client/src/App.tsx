import { useState } from 'react';
import './App.css';
import axios from 'axios';


function App() {
    const [file, setFile] = useState<File | null>(null);
    const [email, setEmail] = useState<string>('');
    const [errorMessage, setErrorMessage] = useState<string>('');

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const selectedFile = event.target.files?.[0];
        if (selectedFile) {
            const fileNameParts = selectedFile.name.split('.');
            const fileExtension = fileNameParts[fileNameParts.length - 1];
            if (fileExtension.toLowerCase() === 'docx') {
                setFile(selectedFile);
                setErrorMessage('');
            } else {
                setErrorMessage('Only .docx files are allowed.');
            }
        }
    };

    const handleEmailChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setEmail(event.target.value);
    };

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        if (!file || !email) {
            setErrorMessage('Please select a file and enter your email address.');
            return;
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await axios.post(`https://traineecampapp.azurewebsites.net/api/BlobStorage?userEmail=${email}`, formData, {
                headers: { 'Content-Type': 'multipart/form-data' },
            });
            console.log('Upload successful:', response.data);
            setFile(null);
            setEmail('');
        } catch (error) {
            console.error('Upload failed:', error);
            setErrorMessage('Failed to upload file. Please try again later.');
        }
    };

    return (
        <div>
            <h2>Upload Form</h2>
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="file">Select .docx file:</label>
                    <input type="file" id="file" accept=".docx" onChange={handleFileChange} />
                </div>
                <div>
                    <label htmlFor="email">Email:</label>
                    <input type="email" id="email" value={email} onChange={handleEmailChange} />
                </div>
                <button type="submit">Upload</button>
            </form>
            {errorMessage && <p style={{ color: 'red' }}>{errorMessage}</p>}
        </div>
    );
}

export default App;