import React from 'react';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: React.ReactNode;
}

const Modal: React.FC<ModalProps> = ({isOpen, onClose, title, children}) => {
    if (!isOpen) return null;

    return (
        <div style={{
            position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex',
            alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }}>
            <div style={{
                backgroundColor: 'white', padding: '20px', borderRadius: '8px',
                width: '90%', maxWidth: '500px', maxHeight: '90vh', overflowY: 'auto'
            }}>
                <div style={{display: 'flex', justifyContent: 'space-between', marginBottom: '15px'}}>
                    <h2 style={{margin: 0}}>{title}</h2>
                    <button onClick={onClose} style={{cursor: 'pointer'}}>✕</button>
                </div>
                {children}
            </div>
        </div>
    );
};

export default Modal;