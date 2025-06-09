import { Spinner } from "react-bootstrap";

export default function FullPageSpinner({ show }: { show: boolean }) {
    if (!show) return null;

    return (
        <div
            style={{
                position: "fixed",
                top: 0,
                left: 0,
                width: "100vw",
                height: "100vh",
                background: "rgba(255,255,255,0.5)",
                zIndex: 2000,
                display: "flex",
                alignItems: "center",
                justifyContent: "center"
            }}
        >
            <Spinner animation="border" variant="primary" style={{ width: 64, height: 64 }} />
        </div>
    );
}