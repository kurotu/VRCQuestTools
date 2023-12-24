import Link from "@docusaurus/Link";
import Translate from "@docusaurus/Translate";

export default function AddToVccLink(props) {
    return (
        <Link
            {...props}
            target={null}
            to="vcc://vpm/addRepo?url=https://kurotu.github.io/vpm-repos/vpm.json">
            <Translate>Add to VCC</Translate>
        </Link>
    );
}
