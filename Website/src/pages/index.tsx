import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageFeatures from '@site/src/components/HomepageFeatures';

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <h1 className={clsx("hero__title", styles.heroTitle)}>{siteConfig.title}</h1>
        <p className={clsx("hero__subtitle", styles.heroSubtitle)}>{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--primary button--lg"
            to="/docs/intro">
            Get Started
          </Link>
          <Link
            className="button button--info button--lg"
            to="vcc://vpm/addRepo?url=https://kurotu.github.io/vpm-repos/vpm.json">
            Add to VCC
          </Link>
          <iframe className={styles.github} src="https://ghbtns.com/github-btn.html?user=kurotu&repo=VRCQuestTools&type=star&count=true&size=large" frameBorder="0" scrolling="0" width="160" height="30" title="GitHub"></iframe>
        </div>
      </div>
    </header>
  );
}

export default function Home(): JSX.Element {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title}`}
      description={`${siteConfig.tagline}`}>
      <HomepageHeader />
      <main>
        <HomepageFeatures />
      </main>
    </Layout>
  );
}
