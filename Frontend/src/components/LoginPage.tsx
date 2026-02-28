import React, {useState} from 'react';
import {authService} from '../api/authService';
import {useAuth} from '../context/AuthContext';

const LoginPage: React.FC = () => {
    const {login} = useAuth();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setLoading(true);

        try {
            const result = await authService.login({username, password});
            login(result.token, result.expires_at);
        } catch (err: any) {
            setError(err.response?.data?.error || 'Login failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{
            display: 'flex', justifyContent: 'center', alignItems: 'center',
            minHeight: '100vh', fontFamily: 'system-ui, sans-serif'
        }}>
            <div style={{
                width: '100%', maxWidth: '380px', padding: '40px',
                border: '1px solid #ddd', borderRadius: '12px',
                backgroundColor: '#fff', boxShadow: '0 2px 12px rgba(0,0,0,0.08)'
            }}>
                <h1 style={{textAlign: 'center', marginTop: 0, marginBottom: '8px', fontSize: '1.6em'}}>
                    🚁 Drone Delivery
                </h1>
                <p style={{textAlign: 'center', color: '#666', marginBottom: '28px', fontSize: '0.9em'}}>
                    Admin Panel Login
                </p>

                <form onSubmit={handleSubmit} style={{display: 'flex', flexDirection: 'column', gap: '14px'}}>
                    <div>
                        <label style={{display: 'block', marginBottom: '4px', fontSize: '0.85em', fontWeight: 500}}>
                            Username
                        </label>
                        <input
                            type="text"
                            value={username}
                            onChange={e => setUsername(e.target.value)}
                            required
                            autoFocus
                            style={{
                                width: '100%', padding: '10px 12px', borderRadius: '6px',
                                border: '1px solid #ccc', fontSize: '1em', boxSizing: 'border-box'
                            }}
                        />
                    </div>
                    <div>
                        <label style={{display: 'block', marginBottom: '4px', fontSize: '0.85em', fontWeight: 500}}>
                            Password
                        </label>
                        <input
                            type="password"
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                            required
                            style={{
                                width: '100%', padding: '10px 12px', borderRadius: '6px',
                                border: '1px solid #ccc', fontSize: '1em', boxSizing: 'border-box'
                            }}
                        />
                    </div>

                    {error && (
                        <p style={{
                            color: '#d32f2f', backgroundColor: '#fdecea',
                            padding: '8px 12px', borderRadius: '6px',
                            margin: 0, fontSize: '0.9em'
                        }}>
                            {error}
                        </p>
                    )}

                    <button
                        type="submit"
                        disabled={loading}
                        style={{
                            padding: '12px', borderRadius: '6px', border: 'none',
                            backgroundColor: '#1976d2', color: '#fff', fontSize: '1em',
                            fontWeight: 600, cursor: loading ? 'not-allowed' : 'pointer',
                            opacity: loading ? 0.7 : 1
                        }}
                    >
                        {loading ? 'Signing in…' : 'Sign In'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default LoginPage;
