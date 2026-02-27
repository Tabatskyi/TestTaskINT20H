import React, {useState} from 'react';
import {ordersService} from '../api/ordersService';
import {ImportOrdersResponse} from '../api/types';

const CsvImport: React.FC = () => {
    const [file, setFile] = useState<File | null>(null);
    const [loading, setLoading] = useState(false);
    const [report, setReport] = useState<ImportOrdersResponse | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setFile(e.target.files[0]);
            setError(null);
        }
    };

    const handleUpload = async () => {
        if (!file) {
            setError("Будь ласка, виберіть файл");
            return;
        }

        setLoading(true);
        setError(null);
        setReport(null);

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
        <div style={{padding: '20px', border: '1px solid #ccc', borderRadius: '8px'}}>
            <h2>Імпорт замовлень (CSV)</h2>
            <div style={{marginBottom: '10px'}}>
                <input type="file" accept=".csv" onChange={handleFileChange}/>
                <button onClick={handleUpload} disabled={loading || !file}>
                    {loading ? 'Завантаження...' : 'Завантажити та обробити'}
                </button>
            </div>

            {error && <p style={{color: 'red'}}>{error}</p>}

            {report && (
                <div style={{backgroundColor: '#eef2ff', padding: '15px', borderRadius: '5px'}}>
                    <p><strong>{report.message}</strong></p>
                    <ul>
                        <li>Імпортовано успішно: {report.importedCount}</li>
                        <li>Пропущено рядків: {report.skippedCount}</li>
                        <li>Час обробки: {report.processingTimeMs} мс</li>
                    </ul>
                    {report.skippedRows.length > 0 && (
                        <details>
                            <summary>Переглянути номери пропущених рядків</summary>
                            <p style={{fontSize: '12px'}}>{report.skippedRows.join(', ')}</p>
                        </details>
                    )}
                </div>
            )}
        </div>
    );
};

export default CsvImport;