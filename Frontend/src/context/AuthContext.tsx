import React, {createContext, useContext, useState, useEffect, useCallback} from 'react';

interface AuthContextType {
    token: string | null;
    isAuthenticated: boolean;
    login: (token: string, expiresAt: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
    token: null,
    isAuthenticated: false,
    login: () => {},
    logout: () => {},
});

export const useAuth = () => useContext(AuthContext);

export const AuthProvider: React.FC<{children: React.ReactNode}> = ({children}) => {
    const [token, setToken] = useState<string | null>(() => {
        const stored = localStorage.getItem('jwt_token');
        const expiresAt = localStorage.getItem('jwt_expires_at');
        if (stored && expiresAt && new Date(expiresAt) > new Date()) {
            return stored;
        }
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('jwt_expires_at');
        return null;
    });

    const login = useCallback((newToken: string, expiresAt: string) => {
        localStorage.setItem('jwt_token', newToken);
        localStorage.setItem('jwt_expires_at', expiresAt);
        setToken(newToken);
    }, []);

    const logout = useCallback(() => {
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('jwt_expires_at');
        setToken(null);
    }, []);

    // Check token expiry periodically
    useEffect(() => {
        const interval = setInterval(() => {
            const expiresAt = localStorage.getItem('jwt_expires_at');
            if (expiresAt && new Date(expiresAt) <= new Date()) {
                logout();
            }
        }, 30000);
        return () => clearInterval(interval);
    }, [logout]);

    return (
        <AuthContext.Provider value={{token, isAuthenticated: !!token, login, logout}}>
            {children}
        </AuthContext.Provider>
    );
};
