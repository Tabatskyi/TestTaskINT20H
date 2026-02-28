import React, {useState} from 'react';
import {ordersService} from '../api/ordersService';
import {ImportOrdersResponse} from '../api/types';
import '../CsvImport.css';

const CsvImport: React.FC = () => {
    const [file, setFile] = useState<File | null>(null);
    const [loading, setLoading] = useState(false);
    const [report, setReport] = useState<ImportOrdersResponse | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            setFile(e.target.files[0]);
            setError(null);
            setReport(null);
        }
    };

    const handleUpload = async () => {
        if (!file) {
            setError("Будь ласка, виберіть файл");
            return;
        }
        setLoading(true);
        setError(null);
        try {
            const data = await ordersService.importCsv(file);
            setReport(data);
        } catch (err: any) {
            setError(err.response?.data?.error || "Помилка при завантаженні файлу");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="csv-import-container">
            <div className={`upload-zone ${file ? 'file-selected' : ''}`}>
                <input
                    type="file"
                    id="csv-file"
                    accept=".csv"
                    onChange={handleFileChange}
                    className="file-input-hidden"
                />
                <label htmlFor="csv-file" className="file-label">
                    <span className="upload-icon">📄</span>
                    {file ? <strong>{file.name}</strong> : "Оберіть або перетягніть CSV файл"}
                </label>
            </div>

            <div className="action-bar">
                <button
                    onClick={handleUpload}
                    disabled={loading || !file}
                    className={`import-btn ${loading ? 'loading' : ''}`}
                >
                    {loading ? 'Обробка...' : 'Завантажити та обробити'}
                </button>
            </div>

            {error && <div className="import-error-msg">{error}</div>}

            {report && (
                <div className="import-report-card fade-in">
                    <div className="report-header">
                        <span className="success-icon">✅</span>
                        <strong>{report.message}</strong>
                    </div>
                    <div className="report-stats">
                        <div className="stat-item">
                            <span>Успішно:</span>
                            <span className="stat-value green">{report.importedCount}</span>
                        </div>
                        <div className="stat-item">
                            <span>Пропущено:</span>
                            <span className="stat-value yellow">{report.skippedCount}</span>
                        </div>
                        <div className="stat-item">
                            <span>Час:</span>
                            <span>{report.processingTimeMs} мс</span>
                        </div>
                    </div>
                    {report.skippedRows.length > 0 && (
                        <details className="skipped-details">
                            <summary>Номери пропущених рядків</summary>
                            <p>{report.skippedRows.join(', ')}</p>
                        </details>
                    )}
                </div>
            )}
        </div>
    );
};

export default CsvImport;